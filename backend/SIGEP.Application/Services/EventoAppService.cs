using SIGEP.Application.DTOs;
using SIGEP.Application.Interfaces;
using SIGEP.Domain.Common;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;
using SIGEP.Domain.Repositories;
using SIGEP.Domain.Services;

namespace SIGEP.Application.Services;

/// <summary>
/// Orquesta la gestión de Eventos Especiales y el Algoritmo de Scoring Ponderado (RF14 - RF18).
/// </summary>
public class EventoAppService : IEventoAppService
{
    private readonly IEventoRepository _eventoRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IEventScoringDomainService _scoringDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public EventoAppService(
        IEventoRepository eventoRepository,
        IEmpleadoRepository empleadoRepository,
        IEventScoringDomainService scoringDomainService,
        IUnitOfWork unitOfWork)
    {
        _eventoRepository = eventoRepository;
        _empleadoRepository = empleadoRepository;
        _scoringDomainService = scoringDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventoDto> CrearEventoAsync(CrearEventoRequestDto request, int idUsuarioActual)
    {
        var evento = new Evento(
            request.Nombre,
            request.FechaInicio,
            request.FechaFin,
            idUsuarioActual,
            request.Descripcion,
            request.Lugar);

        if (request.Turnos != null)
        {
            foreach (var t in request.Turnos)
            {
                evento.AgregarTurno(t.Nombre, t.Fecha, t.HoraInicio, t.HoraFin, t.CupoRequerido);
            }
        }

        await _eventoRepository.AddAsync(evento);
        await _unitOfWork.SaveChangesAsync();

        var creado = await _eventoRepository.GetConDetallesByIdAsync(evento.Id);
        return MapToDto(creado!);
    }

    public async Task<EventoDto> ObtenerPorIdAsync(int id)
    {
        var evento = await _eventoRepository.GetConDetallesByIdAsync(id)
            ?? throw new KeyNotFoundException($"Evento con ID {id} no encontrado.");

        return MapToDto(evento);
    }

    public async Task<List<EventoResumenDto>> ObtenerEventosActivosAsync()
    {
        var eventos = await _eventoRepository.GetEventosActivosAsync();
        return eventos.Select(e => new EventoResumenDto
        {
            Id = e.Id,
            Nombre = e.Nombre,
            FechaInicio = e.FechaInicio,
            FechaFin = e.FechaFin,
            Lugar = e.Lugar,
            Estado = e.Estado.ToString(),
            TotalTurnos = e.Turnos.Count,
            TotalPersonalAsignado = e.Turnos.Sum(t => t.Asignaciones.Count(a => a.Estado == EstadoAsignacionEvento.Confirmado || a.Estado == EstadoAsignacionEvento.Propuesto))
        }).ToList();
    }

    public async Task<List<EventoResumenDto>> ObtenerHistorialAsync(int limite = 20)
    {
        var eventos = await _eventoRepository.GetHistorialEventosAsync(limite);
        return eventos.Select(e => new EventoResumenDto
        {
            Id = e.Id,
            Nombre = e.Nombre,
            FechaInicio = e.FechaInicio,
            FechaFin = e.FechaFin,
            Lugar = e.Lugar,
            Estado = e.Estado.ToString(),
            TotalTurnos = e.Turnos.Count,
            TotalPersonalAsignado = e.Turnos.Sum(t => t.Asignaciones.Count)
        }).ToList();
    }

    public async Task<PropuestaScoringTurnoResponseDto> GenerarPropuestaScoringAsync(int idTurnoEvento)
    {
        var (evento, turno) = await ObtenerTurnoConEventoAsync(idTurnoEvento);

        var todosLosEmpleados = (await _empleadoRepository.GetAllAsync()).ToList();
        var activos = todosLosEmpleados.Where(e => e.Estado == EstadoEmpleado.Activo).ToList();

        var historico = await _eventoRepository.ObtenerHistoricoParaScoringAsync(turno.Fecha);

        var evaluaciones = _scoringDomainService.EvaluarCandidatosParaTurno(
            turno,
            activos,
            historico,
            DateTime.UtcNow);

        var seleccionados = evaluaciones
            .Where(e => !e.EstaExcluido)
            .Take(turno.CupoRequerido)
            .Select(e => new CandidatoEvaluadoDto
            {
                IdEmpleado = e.Empleado.Id,
                Documento = e.Empleado.Documento,
                NombreCompleto = $"{e.Empleado.Apellido}, {e.Empleado.Nombre}",
                Funcion = e.Empleado.Funcion?.Nombre ?? "Sin función",
                Regimen = e.Empleado.Regimen?.Nombre ?? "Régimen",
                PuntajeTotal = e.PuntajeTotal,
                PuntajeHistorial = e.PuntajeHistorial,
                PuntajeRecencia = e.PuntajeRecencia,
                PuntajeSabados = e.PuntajeSabados,
                PuntajeAntiguedad = e.PuntajeAntiguedad
            }).ToList();

        var excluidos = evaluaciones
            .Where(e => e.EstaExcluido)
            .Select(e => new CandidatoExcluidoDto
            {
                IdEmpleado = e.Empleado.Id,
                Documento = e.Empleado.Documento,
                NombreCompleto = $"{e.Empleado.Apellido}, {e.Empleado.Nombre}",
                Regimen = e.Empleado.Regimen?.Nombre ?? "Régimen",
                MotivoExclusion = e.MotivoExclusion ?? "Excluido por normativa laboral"
            }).ToList();

        return new PropuestaScoringTurnoResponseDto
        {
            IdTurnoEvento = turno.Id,
            NombreTurno = turno.Nombre,
            CupoRequerido = turno.CupoRequerido,
            AsignacionesPropuestas = seleccionados,
            AgentesExcluidosPorDescanso = excluidos
        };
    }

    public async Task<EventoDto> AplicarPropuestaScoringAsync(int idTurnoEvento, int idUsuarioActual)
    {
        var (evento, turno) = await ObtenerTurnoConEventoAsync(idTurnoEvento);

        if (evento.Estado != EstadoEvento.Planificado)
            throw new InvalidOperationException("Solo se pueden aplicar propuestas a eventos en estado Planificado.");

        var propuesta = await GenerarPropuestaScoringAsync(idTurnoEvento);

        foreach (var p in propuesta.AsignacionesPropuestas)
        {
            turno.AgregarAsignacion(p.IdEmpleado, p.PuntajeTotal, esManual: false, motivo: null);
        }

        await _unitOfWork.SaveChangesAsync();

        var actualizado = await _eventoRepository.GetConDetallesByIdAsync(evento.Id);
        return MapToDto(actualizado!);
    }

    public async Task<EventoDto> AjustarAsignacionAsync(AjustarAsignacionEventoRequestDto request, int idUsuarioActual)
    {
        var (evento, turno) = await ObtenerTurnoConEventoAsync(request.IdTurnoEvento);

        if (evento.Estado != EstadoEvento.Planificado)
            throw new InvalidOperationException("Solo se pueden ajustar asignaciones en eventos Planificados.");

        var empleadoEntrante = await _empleadoRepository.GetByIdAsync(request.IdEmpleadoEntrante)
            ?? throw new KeyNotFoundException($"El empleado entrante {request.IdEmpleadoEntrante} no existe.");

        if (empleadoEntrante.Estado != EstadoEmpleado.Activo)
            throw new InvalidOperationException("El empleado entrante debe encontrarse en estado Activo.");

        turno.ReemplazarAsignacion(request.IdEmpleadoSaliente, request.IdEmpleadoEntrante, 0, request.Motivo);
        await _unitOfWork.SaveChangesAsync();

        var actualizado = await _eventoRepository.GetConDetallesByIdAsync(evento.Id);
        return MapToDto(actualizado!);
    }

    public async Task<EventoDto> ConfirmarEventoAsync(int idEvento, int idUsuarioActual)
    {
        var evento = await _eventoRepository.GetConDetallesByIdAsync(idEvento)
            ?? throw new KeyNotFoundException($"Evento {idEvento} no encontrado.");

        evento.Confirmar(idUsuarioActual);
        await _unitOfWork.SaveChangesAsync();

        var actualizado = await _eventoRepository.GetConDetallesByIdAsync(evento.Id);
        return MapToDto(actualizado!);
    }

    private async Task<(Evento Evento, TurnoEvento Turno)> ObtenerTurnoConEventoAsync(int idTurnoEvento)
    {
        var eventos = await _eventoRepository.GetHistorialEventosAsync(100);
        foreach (var e in eventos)
        {
            var t = e.Turnos.FirstOrDefault(x => x.Id == idTurnoEvento);
            if (t != null)
                return (e, t);
        }

        throw new KeyNotFoundException($"Turno de evento con ID {idTurnoEvento} no encontrado.");
    }

    private static EventoDto MapToDto(Evento evento)
    {
        return new EventoDto
        {
            Id = evento.Id,
            Nombre = evento.Nombre,
            Descripcion = evento.Descripcion,
            FechaInicio = evento.FechaInicio,
            FechaFin = evento.FechaFin,
            Lugar = evento.Lugar,
            Estado = evento.Estado.ToString(),
            FechaCreacion = evento.FechaCreacion,
            FechaConfirmacion = evento.FechaConfirmacion,
            UsuarioCreacion = evento.UsuarioCreacion?.Username ?? "Sistema",
            UsuarioConfirmacion = evento.UsuarioConfirmacion?.Username,
            TotalTurnos = evento.Turnos.Count,
            Turnos = evento.Turnos.Select(t => new TurnoEventoDto
            {
                Id = t.Id,
                IdEvento = t.IdEvento,
                Nombre = t.Nombre,
                Fecha = t.Fecha,
                Horario = $"{t.HoraInicio:HH\\:mm} - {t.HoraFin:HH\\:mm}",
                CupoRequerido = t.CupoRequerido,
                TotalAsignados = t.Asignaciones.Count(a => a.Estado != EstadoAsignacionEvento.Cancelado && a.Estado != EstadoAsignacionEvento.Reemplazado),
                Asignaciones = t.Asignaciones.Select(a => new AsignacionEventoDto
                {
                    Id = a.Id,
                    IdEmpleado = a.IdEmpleado,
                    Documento = a.Empleado?.Documento ?? string.Empty,
                    NombreCompleto = a.Empleado != null ? $"{a.Empleado.Apellido}, {a.Empleado.Nombre}" : string.Empty,
                    Funcion = a.Empleado?.Funcion?.Nombre ?? "Sin función",
                    Regimen = a.Empleado?.Regimen?.Nombre ?? "Sin régimen",
                    PuntajeCalculado = a.PuntajeCalculado,
                    Estado = a.Estado.ToString(),
                    EsAsignacionManual = a.EsAsignacionManual,
                    MotivoModificacion = a.MotivoModificacion
                }).ToList()
            }).ToList()
        };
    }
}
