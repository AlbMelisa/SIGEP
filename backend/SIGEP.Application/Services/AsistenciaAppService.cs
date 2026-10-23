using SIGEP.Application.DTOs.Asistencia;
using SIGEP.Application.Interfaces;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Enums;
using SIGEP.Domain.Repositories;

namespace SIGEP.Application.Services;

/// <summary>
/// Servicio de Aplicación de Asistencia.
/// Orquesta la planilla diaria (RF04), la gestión de Francos Compensatorios por sábado y la consulta histórica (RF06, RF07).
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
        // 1. Calcular información de Francos Compensatorios si es día de semana hábil (lunes a viernes)
        bool esDiaHabil = fecha.DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday;
        HashSet<int> trabajaronSabado = new();
        HashSet<int> yaUsaronFranco = new();

        if (esDiaHabil)
        {
            // Sábado inmediatamente anterior a la semana actual
            int diasDesdeSabado = ((int)fecha.DayOfWeek - (int)DayOfWeek.Saturday + 7) % 7;
            if (diasDesdeSabado == 0) diasDesdeSabado = 7;
            var sabadoAnterior = fecha.AddDays(-diasDesdeSabado);

            // Rango lunes a viernes de la semana actual
            int diasDesdeLunes = ((int)fecha.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var lunesSemana = fecha.AddDays(-diasDesdeLunes);
            var viernesSemana = lunesSemana.AddDays(4);

            trabajaronSabado = await _asistenciaRepository.ObtenerEmpleadosQueTrabajaronSabadoAsync(sabadoAnterior, cancellationToken);
            yaUsaronFranco = await _asistenciaRepository.ObtenerEmpleadosConFrancoEnSemanaAsync(lunesSemana, viernesSemana, fecha, cancellationToken);
        }

        // 2. Buscar si ya existen marcaciones guardadas para esta fecha
        var registrosGuardados = (await _asistenciaRepository.GetByFechaAsync(fecha, turnoId, cancellationToken)).ToList();

        if (registrosGuardados.Any())
        {
            return registrosGuardados.Select(a =>
            {
                var (tieneFranco, detalleFranco) = EvaluarFranco(a.EmpleadoId, esDiaHabil, trabajaronSabado, yaUsaronFranco);
                return new AsistenciaItemDto(
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
                    Observacion: a.Observacion,
                    TieneFrancoDisponible: tieneFranco,
                    DetalleFranco: detalleFranco
                );
            });
        }

        // 3. Si no existen registros aún, generamos la grilla inicial basada en los empleados activos y su turno habitual
        var empleadosActivos = (await _empleadoRepository.GetActivosAsync(cancellationToken)).ToList();

        if (turnoId.HasValue && turnoId.Value > 0)
        {
            empleadosActivos = empleadosActivos.Where(e => e.TurnoHabitualId == turnoId.Value).ToList();
        }

        return empleadosActivos.Select(e =>
        {
            var (tieneFranco, detalleFranco) = EvaluarFranco(e.Id, esDiaHabil, trabajaronSabado, yaUsaronFranco);
            return new AsistenciaItemDto(
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
                Observacion: null,
                TieneFrancoDisponible: tieneFranco,
                DetalleFranco: detalleFranco
            );
        });
    }

    public async Task<int> GuardarPlanillaLoteAsync(GuardarPlanillaLoteRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Items == null || !request.Items.Any())
            return 0;

        if (!Enum.TryParse<TipoJornada>(request.TipoJornada, true, out var tipoJornada))
        {
            tipoJornada = TipoJornada.Ordinaria;
        }

        bool esDiaHabil = request.Fecha.DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday;
        HashSet<int> trabajaronSabado = new();
        HashSet<int> yaUsaronFranco = new();
        DateOnly sabadoAnterior = request.Fecha;

        if (esDiaHabil && tipoJornada == TipoJornada.Ordinaria)
        {
            int diasDesdeSabado = ((int)request.Fecha.DayOfWeek - (int)DayOfWeek.Saturday + 7) % 7;
            if (diasDesdeSabado == 0) diasDesdeSabado = 7;
            sabadoAnterior = request.Fecha.AddDays(-diasDesdeSabado);

            int diasDesdeLunes = ((int)request.Fecha.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var lunesSemana = request.Fecha.AddDays(-diasDesdeLunes);
            var viernesSemana = lunesSemana.AddDays(4);

            trabajaronSabado = await _asistenciaRepository.ObtenerEmpleadosQueTrabajaronSabadoAsync(sabadoAnterior, cancellationToken);
            yaUsaronFranco = await _asistenciaRepository.ObtenerEmpleadosConFrancoEnSemanaAsync(lunesSemana, viernesSemana, request.Fecha, cancellationToken);
        }

        int procesados = 0;

        foreach (var item in request.Items)
        {
            if (!Enum.TryParse<EstadoAsistencia>(item.Estado, true, out var estado))
            {
                estado = EstadoAsistencia.Presente;
            }

            // Regla de Negocio: Validación estricta de Franco Compensatorio
            if (estado == EstadoAsistencia.Franco && esDiaHabil && tipoJornada == TipoJornada.Ordinaria)
            {
                if (!trabajaronSabado.Contains(item.EmpleadoId))
                {
                    throw new InvalidOperationException($"El agente (ID: {item.EmpleadoId}) no tiene crédito de franco compensatorio: no registra haber trabajado el sábado anterior ({sabadoAnterior:dd/MM/yyyy}).");
                }

                if (yaUsaronFranco.Contains(item.EmpleadoId))
                {
                    throw new InvalidOperationException($"El agente (ID: {item.EmpleadoId}) ya utilizó su franco compensatorio en otro día de esta misma semana.");
                }

                yaUsaronFranco.Add(item.EmpleadoId);
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
            cancellationToken);

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

    private static (bool TieneFranco, string? Detalle) EvaluarFranco(int empleadoId, bool esDiaHabil, HashSet<int> trabajaronSabado, HashSet<int> yaUsaronFranco)
    {
        if (!esDiaHabil)
            return (false, null);

        bool trabajoSabado = trabajaronSabado.Contains(empleadoId);
        bool yaUsoEnOtroDia = yaUsaronFranco.Contains(empleadoId);

        if (!trabajoSabado)
            return (false, "Sin crédito (no trabajó el sábado anterior).");

        if (yaUsoEnOtroDia)
            return (false, "Ya utilizó su franco compensatorio en esta semana.");

        return (true, "⭐ Franco Compensatorio disponible por sábado trabajado.");
    }
}
