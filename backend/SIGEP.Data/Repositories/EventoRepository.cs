using Microsoft.EntityFrameworkCore;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;
using SIGEP.Domain.Repositories;
using SIGEP.Domain.Services;

namespace SIGEP.Data.Repositories;

public class EventoRepository : Repository<Evento>, IEventoRepository
{
    public EventoRepository(SIGEPDbContext context) : base(context)
    {
    }

    public async Task<Evento?> GetConDetallesByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Eventos
            .Include(e => e.UsuarioCreacion)
            .Include(e => e.UsuarioConfirmacion)
            .Include(e => e.Turnos)
                .ThenInclude(t => t.Asignaciones)
                    .ThenInclude(a => a.Empleado)
                        .ThenInclude(emp => emp.Funcion)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<Evento>> GetEventosActivosAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Eventos
            .Include(e => e.Turnos)
                .ThenInclude(t => t.Asignaciones)
            .Where(e => e.Estado == EstadoEvento.Planificado || e.Estado == EstadoEvento.EnCurso)
            .OrderBy(e => e.FechaInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Evento>> GetHistorialEventosAsync(int limite = 20, CancellationToken cancellationToken = default)
    {
        return await Context.Eventos
            .Include(e => e.UsuarioCreacion)
            .Include(e => e.Turnos)
                .ThenInclude(t => t.Asignaciones)
            .OrderByDescending(e => e.FechaInicio)
            .Take(limite)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, InfoHistoricoEmpleado>> ObtenerHistoricoParaScoringAsync(DateOnly fechaEvento, CancellationToken cancellationToken = default)
    {
        var primerDiaAnio = new DateOnly(fechaEvento.Year, 1, 1);
        var hace90Dias = fechaEvento.AddDays(-90);
        var hace5Dias = fechaEvento.AddDays(-5);

        // 1. Conteo de eventos en el año actual por empleado
        var eventosAnio = await Context.AsignacionesEventos
            .Where(a => a.Estado == EstadoAsignacionEvento.Confirmado && a.TurnoEvento.Fecha >= primerDiaAnio && a.TurnoEvento.Fecha < fechaEvento)
            .GroupBy(a => a.IdEmpleado)
            .Select(g => new { EmpleadoId = g.Key, Cantidad = g.Count(), MaxFecha = g.Max(x => x.TurnoEvento.Fecha) })
            .ToListAsync(cancellationToken);

        // 2. Conteo de sábados en los últimos 90 días
        var sabadosRecientes = await Context.Asistencias
            .Where(a => a.TipoJornada == TipoJornada.Sabado && a.Estado == EstadoAsistencia.Presente && a.Fecha >= hace90Dias && a.Fecha < fechaEvento)
            .GroupBy(a => a.EmpleadoId)
            .Select(g => new { EmpleadoId = g.Key, Cantidad = g.Count() })
            .ToListAsync(cancellationToken);

        // 3. Última guardia nocturna del Régimen Histórico
        var turnosNocheHistorico = await Context.Asistencias
            .Include(a => a.Turno)
            .Where(a => a.Empleado.RegimenId == 2 && a.Estado == EstadoAsistencia.Presente && a.Fecha >= hace5Dias && a.Fecha <= fechaEvento)
            .ToListAsync(cancellationToken);

        var guardiasNoche = turnosNocheHistorico
            .Where(a => a.Turno != null && a.Turno.EsNocturno())
            .GroupBy(a => a.EmpleadoId)
            .Select(g =>
            {
                var ultima = g.OrderByDescending(x => x.Fecha).First();
                // Turno noche de 19:00 a 07:00 finaliza a las 07:00 del día siguiente
                var fechaFin = ultima.Fecha.AddDays(1).ToDateTime(ultima.Turno.HoraFin);
                return new { EmpleadoId = g.Key, FechaFinGuardia = fechaFin };
            })
            .ToList();

        // 4. Asistencia Semanal Ordinaria (Presentismo de lunes a viernes previo al evento)
        int diasHastaLunes = ((int)fechaEvento.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        if (diasHastaLunes == 0) diasHastaLunes = 7;
        var inicioSemanaHabil = fechaEvento.AddDays(-diasHastaLunes);
        var finSemanaHabil = inicioSemanaHabil.AddDays(4);

        var asistenciasSemana = await Context.Asistencias
            .Where(a => a.TipoJornada == TipoJornada.Ordinaria && a.Estado == EstadoAsistencia.Presente && a.Fecha >= inicioSemanaHabil && a.Fecha <= finSemanaHabil)
            .GroupBy(a => a.EmpleadoId)
            .Select(g => new { EmpleadoId = g.Key, Cantidad = g.Count() })
            .ToListAsync(cancellationToken);

        // Consolidar resultados
        var diccionario = new Dictionary<int, InfoHistoricoEmpleado>();

        foreach (var e in eventosAnio)
        {
            if (!diccionario.TryGetValue(e.EmpleadoId, out var info))
            {
                info = new InfoHistoricoEmpleado();
                diccionario[e.EmpleadoId] = info;
            }
            info.CantidadEventosAnioActual = e.Cantidad;
            info.FechaUltimoEvento = e.MaxFecha;
        }

        foreach (var s in sabadosRecientes)
        {
            if (!diccionario.TryGetValue(s.EmpleadoId, out var info))
            {
                info = new InfoHistoricoEmpleado();
                diccionario[s.EmpleadoId] = info;
            }
            info.CantidadSabadosUltimos90Dias = s.Cantidad;
        }

        foreach (var sem in asistenciasSemana)
        {
            if (!diccionario.TryGetValue(sem.EmpleadoId, out var info))
            {
                info = new InfoHistoricoEmpleado();
                diccionario[sem.EmpleadoId] = info;
            }
            info.DiasPresentesSemanaPrevia = sem.Cantidad;
        }

        foreach (var g in guardiasNoche)
        {
            if (!diccionario.TryGetValue(g.EmpleadoId, out var info))
            {
                info = new InfoHistoricoEmpleado();
                diccionario[g.EmpleadoId] = info;
            }
            info.FechaFinUltimaGuardiaNocturna = g.FechaFinGuardia;
        }

        return diccionario;
    }
}

