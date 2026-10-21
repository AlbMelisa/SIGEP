namespace SIGEP.Application.DTOs.Asistencia;

public record AsistenciaItemDto(
    int? AsistenciaId,
    int EmpleadoId,
    string Nombre,
    string Apellido,
    string NombreCompleto,
    string Documento,
    string Funcion,
    int TurnoId,
    string TurnoNombre,
    string TurnoHorario,
    string? HoraIngreso,
    string? HoraSalida,
    string Estado,
    string? Observacion
);

public record GuardarItemAsistenciaDto(
    int EmpleadoId,
    int TurnoId,
    string? HoraIngreso,
    string? HoraSalida,
    string Estado,
    string? Observacion
);

public record GuardarPlanillaLoteRequestDto(
    DateOnly Fecha,
    string TipoJornada, // "Ordinaria", "Sabado", "Evento"
    int? EventoId,
    IEnumerable<GuardarItemAsistenciaDto> Items
);

public record HistorialAsistenciaDto(
    int Id,
    DateOnly Fecha,
    int EmpleadoId,
    string EmpleadoNombre,
    string Documento,
    string Funcion,
    string Regimen,
    int TurnoId,
    string TurnoNombre,
    string TurnoHorario,
    string? HoraIngreso,
    string? HoraSalida,
    string Estado,
    string TipoJornada,
    string? Observacion
);

public record FiltroHistorialRequestDto(
    DateOnly? Desde,
    DateOnly? Hasta,
    int? EmpleadoId,
    int? TurnoId,
    string? Estado,
    string? TipoJornada
);

