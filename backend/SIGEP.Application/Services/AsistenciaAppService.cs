using SIGEP.Application.DTOs.Asistencia;
using SIGEP.Application.Interfaces;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;
using SIGEP.Domain.Repositories;

namespace SIGEP.Application.Services;

/// <summary>
/// Servicio de Aplicación de Asistencia.
/// Orquesta la planilla diaria (RF04) y la consulta histórica (RF06, RF07).
/// </summary>
public class AsistenciaAppService : IAsistenciaAppService
{
    private readonly IAsistenciaRepository _asistenciaRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly ITurnoRepository _turnoRepository;

    public AsistenciaAppService(
        IAsistenciaRepository asistenciaRepository,
        IEmpleadoRepository empleadoRepository,
        ITurnoRepository turnoRepository)
    {
        _asistenciaRepository = asistenciaRepository ?? throw new ArgumentNullException(nameof(asistenciaRepository));
        _empleadoRepository = empleadoRepository ?? throw new ArgumentNullException(nameof(empleadoRepository));
        _turnoRepository = turnoRepository ?? throw new ArgumentNullException(nameof(turnoRepository));
    }

    public async Task<IEnumerable<AsistenciaItemDto>> ObtenerPlanillaDelDiaAsync(DateOnly fecha, int? turnoId, CancellationToken cancellationToken = default)
    {
        // 1. Buscar si ya existen marcaciones guardadas para esta fecha
        var registrosGuardados = (await _asistenciaRepository.GetByFechaAsync(fecha, turnoId, cancellationToken)).ToList();

        // 2. Si ya existen registros, los devolvemos directamente
        if (registrosGuardados.Any())
        {
            return registrosGuardados.Select(a => new AsistenciaItemDto(
                AsistenciaId: a.Id,
                EmpleadoId: a.EmpleadoId,
                Nombre: a.Empleado.Nombre,
                Apellido: a.Empleado.Apellido,
                NombreCompleto: a.Empleado.NombreCompleto,
                Documento: a.Empleado.Documento,
                Funcion: a.Empleado.Funcion?.Nombre ?? "Sin función",
                TurnoId: a.TurnoId,
                TurnoNombre: a.Turno?.Nombre ?? "Sin turno",
                TurnoHorario: a.Turno != null ? $"{a.Turno.HoraInicio:HH:mm} - {a.Turno.HoraFin:HH:mm}" : "",
                HoraIngreso: a.HoraIngreso.HasValue ? a.HoraIngreso.Value.ToString("HH:mm") : null,
                HoraSalida: a.HoraSalida.HasValue ? a.HoraSalida.Value.ToString("HH:mm") : null,
                Estado: a.Estado.ToString(),
                Observacion: a.Observacion
            ));
        }

        // 3. Si no existen registros aún, generamos la grilla inicial basada en los empleados activos y su turno habitual
        var empleadosActivos = (await _empleadoRepository.GetActivosAsync(cancellationToken)).ToList();

        if (turnoId.HasValue && turnoId.Value > 0)
        {
            empleadosActivos = empleadosActivos.Where(e => e.TurnoHabitualId == turnoId.Value).ToList();
        }

        return empleadosActivos.Select(e => new AsistenciaItemDto(
            AsistenciaId: null,
            EmpleadoId: e.Id,
            Nombre: e.Nombre,
            Apellido: e.Apellido,
            NombreCompleto: e.NombreCompleto,
            Documento: e.Documento,
            Funcion: e.Funcion?.Nombre ?? "Sin función",
            TurnoId: e.TurnoHabitualId,
            TurnoNombre: e.TurnoHabitual?.Nombre ?? "Sin turno",
            TurnoHorario: e.TurnoHabitual != null ? $"{e.TurnoHabitual.HoraInicio:HH:mm} - {e.TurnoHabitual.HoraFin:HH:mm}" : "",
            HoraIngreso: null,
            HoraSalida: null,
            Estado: EstadoAsistencia.Presente.ToString(),
            Observacion: null
        ));
    }

    public async Task<int> GuardarPlanillaLoteAsync(GuardarPlanillaLoteRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Items == null || !request.Items.Any())
            return 0;

        // Parsear el TipoJornada
        if (!Enum.TryParse<TipoJornada>(request.TipoJornada, true, out var tipoJornada))
        {
            tipoJornada = TipoJornada.Ordinaria;
        }

        int procesados = 0;

        foreach (var item in request.Items)
        {
            // Parsear estado
            if (!Enum.TryParse<EstadoAsistencia>(item.Estado, true, out var estado))
            {
                estado = EstadoAsistencia.Presente;
            }

            TimeOnly? horaIngreso = null;
            if (!string.IsNullOrWhiteSpace(item.HoraIngreso) && TimeOnly.TryParse(item.HoraIngreso, out var parsedIngreso))
            {
                horaIngreso = parsedIngreso;
            }

            TimeOnly? horaSalida = null;
            if (!string.IsNullOrWhiteSpace(item.HoraSalida) && TimeOnly.TryParse(item.HoraSalida, out var parsedSalida))
            {
                horaSalida = parsedSalida;
            }

            // Buscar si ya existe el registro para este empleado, fecha y turno
            var existente = await _asistenciaRepository.GetByEmpleadoYFechaAsync(item.EmpleadoId, request.Fecha, cancellationToken);

            if (existente != null)
            {
                existente.ActualizarMarcacion(horaIngreso, horaSalida, estado, item.Observacion);
                _asistenciaRepository.Update(existente);
            }
            else
            {
                var nuevaAsistencia = new Asistencia(
                    empleadoId: item.EmpleadoId,
                    fecha: request.Fecha,
                    turnoId: item.TurnoId,
                    estado: estado,
                    tipoJornada: tipoJornada,
                    horaIngreso: horaIngreso,
                    horaSalida: horaSalida,
                    eventoId: request.EventoId,
                    observacion: item.Observacion
                );
                await _asistenciaRepository.AddAsync(nuevaAsistencia, cancellationToken);
            }

            procesados++;
        }

        await _asistenciaRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return procesados;
    }

    public async Task<IEnumerable<HistorialAsistenciaDto>> ConsultarHistorialAsync(FiltroHistorialRequestDto filtro, CancellationToken cancellationToken = default)
    {
        EstadoAsistencia? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(filtro.Estado) && Enum.TryParse<EstadoAsistencia>(filtro.Estado, true, out var parsedEstado))
        {
            estadoEnum = parsedEstado;
        }

        TipoJornada? tipoJornadaEnum = null;
        if (!string.IsNullOrWhiteSpace(filtro.TipoJornada) && Enum.TryParse<TipoJornada>(filtro.TipoJornada, true, out var parsedTipo))
        {
            tipoJornadaEnum = parsedTipo;
        }

        var historial = await _asistenciaRepository.GetHistorialAsync(
            filtro.Desde,
            filtro.Hasta,
            filtro.EmpleadoId,
            filtro.TurnoId,
            estadoEnum,
            tipoJornadaEnum,
            cancellationToken
        );

        return historial.Select(a => new HistorialAsistenciaDto(
            Id: a.Id,
            Fecha: a.Fecha,
            EmpleadoId: a.EmpleadoId,
            EmpleadoNombre: a.Empleado.NombreCompleto,
            Documento: a.Empleado.Documento,
            Funcion: a.Empleado.Funcion?.Nombre ?? "Sin función",
            Regimen: a.Empleado.Regimen?.Nombre ?? "Sin régimen",
            TurnoId: a.TurnoId,
            TurnoNombre: a.Turno?.Nombre ?? "Sin turno",
            TurnoHorario: a.Turno != null ? $"{a.Turno.HoraInicio:HH:mm} - {a.Turno.HoraFin:HH:mm}" : "",
            HoraIngreso: a.HoraIngreso.HasValue ? a.HoraIngreso.Value.ToString("HH:mm") : null,
            HoraSalida: a.HoraSalida.HasValue ? a.HoraSalida.Value.ToString("HH:mm") : null,
            Estado: a.Estado.ToString(),
            TipoJornada: a.TipoJornada.ToString(),
            Observacion: a.Observacion
        ));
    }
}

