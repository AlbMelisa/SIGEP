namespace SIGEP.Application.DTOs.Personal;

public record EmpleadoDto(
    int Id,
    string Nombre,
    string Apellido,
    string NombreCompleto,
    string Documento,
    string? Direccion,
    string? Telefono,
    string? Email,
    int FuncionId,
    string FuncionNombre,
    int RegimenId,
    string RegimenNombre,
    int TurnoHabitualId,
    string TurnoHabitualNombre,
    string TurnoHorario,
    string Estado,
    DateTime FechaIngreso
);

public record CrearEmpleadoRequestDto(
    string Nombre,
    string Apellido,
    string Documento,
    int FuncionId,
    int RegimenId,
    int TurnoHabitualId,
    string? Direccion = null,
    string? Telefono = null,
    string? Email = null
);

public record ActualizarEmpleadoRequestDto(
    string Nombre,
    string Apellido,
    string Documento,
    int FuncionId,
    int RegimenId,
    int TurnoHabitualId,
    string? Direccion = null,
    string? Telefono = null,
    string? Email = null
);

public record OpcionReferenciaDto(int Id, string Nombre, string? Descripcion = null);

public record TurnoReferenciaDto(int Id, string Nombre, string Horario, int RegimenId);

public record ListasReferenciaDto(
    IEnumerable<OpcionReferenciaDto> Funciones,
    IEnumerable<OpcionReferenciaDto> Regimenes,
    IEnumerable<TurnoReferenciaDto> Turnos
);

