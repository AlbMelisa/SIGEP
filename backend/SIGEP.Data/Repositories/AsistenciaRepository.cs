using Microsoft.EntityFrameworkCore;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;
using SIGEP.Domain.Repositories;

namespace SIGEP.Data.Repositories;

/// <summary>
/// Implementación con Entity Framework Core del Repositorio de Asistencias.
/// Optimizado para consultas de planilla diaria y filtros complejos de historial (RF06).
/// </summary>
public class AsistenciaRepository : Repository<Asistencia>, IAsistenciaRepository
{
    public AsistenciaRepository(SIGEPDbContext context) : base(context)
    {
    }

    public async Task<Asistencia?> GetByEmpleadoYFechaAsync(int empleadoId, DateOnly fecha, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Empleado)
            .Include(a => a.Turno)
            .FirstOrDefaultAsync(a => a.EmpleadoId == empleadoId && a.Fecha == fecha, cancellationToken);
    }

    public async Task<IEnumerable<Asistencia>> GetByFechaAsync(DateOnly fecha, int? turnoId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(a => a.Empleado)
                .ThenInclude(e => e.Funcion)
            .Include(a => a.Turno)
            .Where(a => a.Fecha == fecha);

        if (turnoId.HasValue && turnoId.Value > 0)
        {
            query = query.Where(a => a.TurnoId == turnoId.Value);
        }

        return await query
            .OrderBy(a => a.Turno.HoraInicio)
            .ThenBy(a => a.Empleado.Apellido)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Asistencia>> GetHistorialAsync(
        DateOnly? desde,
        DateOnly? hasta,
        int? empleadoId,
        int? turnoId,
        EstadoAsistencia? estado,
        TipoJornada? tipoJornada,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(a => a.Empleado)
                .ThenInclude(e => e.Funcion)
            .Include(a => a.Empleado)
                .ThenInclude(e => e.Regimen)
            .Include(a => a.Turno)
            .AsQueryable();

        if (desde.HasValue)
        {
            query = query.Where(a => a.Fecha >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(a => a.Fecha <= hasta.Value);
        }

        if (empleadoId.HasValue && empleadoId.Value > 0)
        {
            query = query.Where(a => a.EmpleadoId == empleadoId.Value);
        }

        if (turnoId.HasValue && turnoId.Value > 0)
        {
            query = query.Where(a => a.TurnoId == turnoId.Value);
        }

        if (estado.HasValue)
        {
            query = query.Where(a => a.Estado == estado.Value);
        }

        if (tipoJornada.HasValue)
        {
            query = query.Where(a => a.TipoJornada == tipoJornada.Value);
        }

        return await query
            .OrderByDescending(a => a.Fecha)
            .ThenBy(a => a.Turno.HoraInicio)
            .ThenBy(a => a.Empleado.Apellido)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Asistencia>> GetAsistenciasRecientesAsync(int empleadoId, DateOnly fechaReferencia, int diasAtras = 30, CancellationToken cancellationToken = default)
    {
        var fechaLimite = fechaReferencia.AddDays(-diasAtras);
        return await DbSet
            .Where(a => a.EmpleadoId == empleadoId && a.Fecha >= fechaLimite && a.Fecha <= fechaReferencia)
            .ToListAsync(cancellationToken);
    }

    public async Task<HashSet<int>> ObtenerEmpleadosQueTrabajaronSabadoAsync(DateOnly sabado, CancellationToken cancellationToken = default)
    {
        // 1. Empleados con asistencia 'Presente' en sábado
        var desdeAsistencias = await DbSet
            .Where(a => a.Fecha == sabado && a.TipoJornada == TipoJornada.Sabado && a.Estado == EstadoAsistencia.Presente)
            .Select(a => a.EmpleadoId)
            .ToListAsync(cancellationToken);

        // 2. Empleados en planificación confirmada de sábado
        var desdePlanificaciones = await Context.DetallesPlanificacionSabado
            .Where(d => d.PlanificacionSabado.FechaSabado == sabado && d.PlanificacionSabado.Estado == EstadoPlanificacion.Confirmada)
            .Select(d => d.IdEmpleado)
            .ToListAsync(cancellationToken);

        var resultado = new HashSet<int>(desdeAsistencias);
        foreach (var id in desdePlanificaciones)
        {
            resultado.Add(id);
        }

        return resultado;
    }

    public async Task<HashSet<int>> ObtenerEmpleadosConFrancoEnSemanaAsync(DateOnly inicioSemana, DateOnly finSemana, DateOnly fechaExcluir, CancellationToken cancellationToken = default)
    {
        var francos = await DbSet
            .Where(a => a.Fecha >= inicioSemana && a.Fecha <= finSemana && a.Fecha != fechaExcluir && a.Estado == EstadoAsistencia.Franco)
            .Select(a => a.EmpleadoId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new HashSet<int>(francos);
    }
}

