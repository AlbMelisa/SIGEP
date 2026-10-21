using SIGEP.Application.DTOs;
using SIGEP.Application.Interfaces;
using SIGEP.Domain.Common;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;
using SIGEP.Domain.Repositories;
using SIGEP.Domain.Services;

namespace SIGEP.Application.Services;

/// <summary>
/// Orquesta los casos de uso para la Planificación Inteligente de Sábados (RF08 - RF13).
/// </summary>
public class PlanificacionSabadoAppService : IPlanificacionSabadoAppService
{
    private readonly IPlanificacionSabadoRepository _planificacionRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly ITurnoRepository _turnoRepository;
    private readonly ISabadoRotacionDomainService _sabadoRotacionDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public PlanificacionSabadoAppService(
        IPlanificacionSabadoRepository planificacionRepository,
        IEmpleadoRepository empleadoRepository,
        ITurnoRepository turnoRepository,
        ISabadoRotacionDomainService sabadoRotacionDomainService,
        IUnitOfWork unitOfWork)
    {
        _planificacionRepository = planificacionRepository;
        _empleadoRepository = empleadoRepository;
        _turnoRepository = turnoRepository;
        _sabadoRotacionDomainService = sabadoRotacionDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PlanificacionSabadoDto> GenerarPropuestaAsync(GenerarPropuestaSabadoRequestDto request, int idUsuarioActual)
    {
        if (request.FechaSabado.DayOfWeek != DayOfWeek.Saturday)
            throw new ArgumentException($"La fecha {request.FechaSabado:dd/MM/yyyy} debe ser un sábado.", nameof(request.FechaSabado));

        var existente = await _planificacionRepository.GetByFechaAsync(request.FechaSabado);
        if (existente != null)
        {
            if (existente.Estado == EstadoPlanificacion.Confirmada)
                throw new InvalidOperationException($"Ya existe una planificación formalmente confirmada para el sábado {request.FechaSabado:dd/MM/yyyy}.");

            throw new InvalidOperationException($"Ya existe un borrador generado para el sábado {request.FechaSabado:dd/MM/yyyy}. Puede modificarlo o consultarlo directamente.");
        }

        // Obtener dotación activa del Régimen Incorporado (RegimenId = 1)
        var todosLosEmpleados = await _empleadoRepository.GetAllAsync();
        var incorporadosActivos = todosLosEmpleados
            .Where(e => e.RegimenId == 1 && e.Estado == EstadoEmpleado.Activo)
            .ToList();

        if (!incorporadosActivos.Any())
            throw new InvalidOperationException("No se encontraron agentes activos pertenecientes al Régimen Incorporado.");

        // Obtener turnos operativos del Régimen Incorporado
        var turnosDisponibles = (await _turnoRepository.GetByRegimenIdAsync(1)).ToList();
        if (!turnosDisponibles.Any())
            throw new InvalidOperationException("No se encontraron turnos operativos configurados para el Régimen Incorporado.");

        // Obtener historial quincenal para el algoritmo de alternancia
        var ultimoSabado = await _planificacionRepository.GetUltimoSabadoTrabajadoPorEmpleadoAsync();
        var ultimoTurno = await _planificacionRepository.GetUltimoTurnoSabadoPorEmpleadoAsync();

        // Ejecutar Algoritmo de Dominio Puro
        var plan = _sabadoRotacionDomainService.GenerarPropuestaPreliminar(
            request.FechaSabado,
            idUsuarioActual,
            incorporadosActivos,
            turnosDisponibles,
            ultimoSabado,
            ultimoTurno,
            request.Observaciones);

        await _planificacionRepository.AddAsync(plan);
        await _unitOfWork.SaveChangesAsync();

        var guardado = await _planificacionRepository.GetConDetallesByIdAsync(plan.Id);
        return MapToDto(guardado!);
    }

    public async Task<PlanificacionSabadoDto?> ObtenerPorFechaAsync(DateOnly fechaSabado)
    {
        var plan = await _planificacionRepository.GetByFechaAsync(fechaSabado);
        return plan != null ? MapToDto(plan) : null;
    }

    public async Task<PlanificacionSabadoDto> ObtenerPorIdAsync(int id)
    {
        var plan = await _planificacionRepository.GetConDetallesByIdAsync(id)
            ?? throw new KeyNotFoundException($"Planificación con ID {id} no encontrada.");

        return MapToDto(plan);
    }

    public async Task<List<PlanificacionSabadoResumenDto>> ObtenerHistorialAsync(int limite = 15)
    {
        var lista = await _planificacionRepository.GetHistorialAsync(limite);
        return lista.Select(p => new PlanificacionSabadoResumenDto
        {
            Id = p.Id,
            FechaSabado = p.FechaSabado,
            Estado = p.Estado.ToString(),
            TotalAsignados = p.Detalles.Count,
            FechaCreacion = p.FechaCreacion,
            FechaConfirmacion = p.FechaConfirmacion,
            UsuarioCreacion = p.UsuarioCreacion?.Username ?? "Sistema"
        }).ToList();
    }

    public async Task<PlanificacionSabadoDto> ModificarTurnoAsync(ModificarTurnoSabadoRequestDto request, int idUsuarioActual)
    {
        var plan = await _planificacionRepository.GetConDetallesByIdAsync(request.IdPlanificacion)
            ?? throw new KeyNotFoundException($"Planificación {request.IdPlanificacion} no encontrada.");

        var turno = await _turnoRepository.GetByIdAsync(request.NuevoIdTurno)
            ?? throw new KeyNotFoundException($"Turno {request.NuevoIdTurno} no encontrado.");

        if (turno.RegimenId != 1)
            throw new InvalidOperationException("El nuevo turno debe pertenecer al Régimen Incorporado.");

        plan.ModificarTurnoEmpleado(request.IdEmpleado, request.NuevoIdTurno, request.Motivo);
        await _unitOfWork.SaveChangesAsync();

        var actualizado = await _planificacionRepository.GetConDetallesByIdAsync(plan.Id);
        return MapToDto(actualizado!);
    }

    public async Task<PlanificacionSabadoDto> ReemplazarEmpleadoAsync(ReemplazarEmpleadoSabadoRequestDto request, int idUsuarioActual)
    {
        var plan = await _planificacionRepository.GetConDetallesByIdAsync(request.IdPlanificacion)
            ?? throw new KeyNotFoundException($"Planificación {request.IdPlanificacion} no encontrada.");

        var empleadoEntrante = await _empleadoRepository.GetByIdAsync(request.IdEmpleadoEntrante)
            ?? throw new KeyNotFoundException($"El empleado entrante {request.IdEmpleadoEntrante} no existe.");

        if (empleadoEntrante.RegimenId != 1)
            throw new InvalidOperationException("El empleado de reemplazo debe pertenecer al Régimen Incorporado.");

        if (empleadoEntrante.Estado != EstadoEmpleado.Activo)
            throw new InvalidOperationException("El empleado de reemplazo debe encontrarse en estado Activo.");

        var turno = await _turnoRepository.GetByIdAsync(request.IdTurno)
            ?? throw new KeyNotFoundException($"Turno {request.IdTurno} no encontrado.");

        plan.ReemplazarEmpleado(request.IdEmpleadoSaliente, request.IdEmpleadoEntrante, request.IdTurno, request.Motivo);
        await _unitOfWork.SaveChangesAsync();

        var actualizado = await _planificacionRepository.GetConDetallesByIdAsync(plan.Id);
        return MapToDto(actualizado!);
    }

    public async Task<PlanificacionSabadoDto> ConfirmarAsync(int idPlanificacion, int idUsuarioActual)
    {
        var plan = await _planificacionRepository.GetConDetallesByIdAsync(idPlanificacion)
            ?? throw new KeyNotFoundException($"Planificación {idPlanificacion} no encontrada.");

        plan.Confirmar(idUsuarioActual);
        await _unitOfWork.SaveChangesAsync();

        var actualizado = await _planificacionRepository.GetConDetallesByIdAsync(plan.Id);
        return MapToDto(actualizado!);
    }

    private static PlanificacionSabadoDto MapToDto(PlanificacionSabado plan)
    {
        return new PlanificacionSabadoDto
        {
            Id = plan.Id,
            FechaSabado = plan.FechaSabado,
            Estado = plan.Estado.ToString(),
            Observaciones = plan.Observaciones,
            FechaCreacion = plan.FechaCreacion,
            FechaConfirmacion = plan.FechaConfirmacion,
            UsuarioCreacion = plan.UsuarioCreacion?.Username ?? "Sistema",
            UsuarioConfirmacion = plan.UsuarioConfirmacion?.Username,
            TotalAsignados = plan.Detalles.Count,
            Detalles = plan.Detalles.Select(d => new DetallePlanificacionSabadoDto
            {
                Id = d.Id,
                IdEmpleado = d.IdEmpleado,
                Documento = d.Empleado?.Documento ?? string.Empty,
                NombreCompleto = d.Empleado != null ? $"{d.Empleado.Apellido}, {d.Empleado.Nombre}" : string.Empty,
                Funcion = d.Empleado?.Funcion?.Nombre ?? "Sin función",
                IdTurno = d.IdTurno,
                NombreTurno = d.Turno?.Nombre ?? "Turno",
                HorarioTurno = d.Turno != null ? $"{d.Turno.HoraInicio:HH\\:mm} - {d.Turno.HoraFin:HH\\:mm}" : string.Empty,
                EsModificacionManual = d.EsModificacionManual,
                MotivoModificacion = d.MotivoModificacion
            }).ToList()
        };
    }
}
