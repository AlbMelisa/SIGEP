namespace SIGEP.Domain.Enums;

/// <summary>
/// Estados posibles de asistencia de un empleado para una jornada (RF05).
/// </summary>
public enum EstadoAsistencia
{
    Presente = 1,
    Ausente = 2,
    Franco = 3,
    NotaMedica = 4,
    Licencia = 5,
    PermisoEspecial = 6
}

