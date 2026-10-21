using Microsoft.EntityFrameworkCore;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;
using SIGEP.Domain.Repositories;

namespace SIGEP.Data.Repositories;

public class PlanificacionSabadoRepository : Repository<PlanificacionSabado>, IPlanificacionSabadoRepository
{
    public PlanificacionSabadoRepository(SIGEPDbContext context) : base(context)
    {
    }

    public async Task<PlanificacionSabado?> GetByFechaAsync(DateOnly fechaSabado, CancellationToken cancellationToken = default)
    {
        return await Context.PlanificacionesSabado
            .Include(p => p.UsuarioCreacion)
            .Include(p => p.UsuarioConfirmacion)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Empleado)
                    .ThenInclude(e => e.Funcion)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Turno)
            .FirstOrDefaultAsync(p => p.FechaSabado == fechaSabado, cancellationToken);
    }

    public async Task<PlanificacionSabado?> GetConDetallesByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.PlanificacionesSabado
            .Include(p => p.UsuarioCreacion)
            .Include(p => p.UsuarioConfirmacion)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Empleado)
                    .ThenInclude(e => e.Funcion)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Turno)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PlanificacionSabado?> GetUltimaConfirmadaAntesDeAsync(DateOnly fecha, CancellationToken cancellationToken = default)
    {
        return await Context.PlanificacionesSabado
            .Include(p => p.Detalles)
            .Where(p => p.FechaSabado < fecha && p.Estado == EstadoPlanificacion.Confirmada)
            .OrderByDescending(p => p.FechaSabado)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<PlanificacionSabado>> GetHistorialAsync(int limite = 15, CancellationToken cancellationToken = default)
    {
        return await Context.PlanificacionesSabado
            .Include(p => p.UsuarioCreacion)
            .Include(p => p.UsuarioConfirmacion)
            .Include(p => p.Detalles)
            .OrderByDescending(p => p.FechaSabado)
            .Take(limite)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, DateOnly>> GetUltimoSabadoTrabajadoPorEmpleadoAsync(CancellationToken cancellationToken = default)
    {
        // 1. Obtener de planificaciones confirmadas
        var desdePlanificaciones = await Context.DetallesPlanificacionSabado
            .Where(d => d.PlanificacionSabado.Estado == EstadoPlanificacion.Confirmada)
            .GroupBy(d => d.IdEmpleado)
            .Select(g => new { EmpleadoId = g.Key, MaxFecha = g.Max(d => d.PlanificacionSabado.FechaSabado) })
            .ToListAsync(cancellationToken);

        // 2. Obtener de asistencias reales de sábados
        var desdeAsistencias = await Context.Asistencias
            .Where(a => a.TipoJornada == TipoJornada.Sabado && a.Estado != EstadoAsistencia.Ausente)
            .GroupBy(a => a.EmpleadoId)
            .Select(g => new { EmpleadoId = g.Key, MaxFecha = g.Max(a => a.Fecha) })
            .ToListAsync(cancellationToken);

        // Combinar tomando la fecha más reciente registrada
        var resultado = new Dictionary<int, DateOnly>();

        foreach (var item in desdePlanificaciones)
        {
            resultado[item.EmpleadoId] = item.MaxFecha;
        }

        foreach (var item in desdeAsistencias)
        {
            if (!resultado.TryGetValue(item.EmpleadoId, out var actual) || item.MaxFecha > actual)
            {
                resultado[item.EmpleadoId] = item.MaxFecha;
            }
        }

        return resultado;
    }

    public async Task<Dictionary<int, int>> GetUltimoTurnoSabadoPorEmpleadoAsync(CancellationToken cancellationToken = default)
    {
        // Obtener el turno asignado en el último sábado confirmado
        var detalles = await Context.DetallesPlanificacionSabado
            .Where(d => d.PlanificacionSabado.Estado == EstadoPlanificacion.Confirmada)
            .Select(d => new
            {
                d.IdEmpleado,
                d.IdTurno,
                d.PlanificacionSabado.FechaSabado
            })
            .ToListAsync(cancellationToken);

        return detalles
            .GroupBy(d => d.IdEmpleado)
            .Select(g => g.OrderByDescending(x => x.FechaSabado).First())
            .ToDictionary(x => x.IdEmpleado, x => x.IdTurno);
    }
}

