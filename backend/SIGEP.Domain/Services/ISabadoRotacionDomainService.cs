using SIGEP.Domain.Entities;

namespace SIGEP.Domain.Services;

/// <summary>
/// Contrato del Servicio de Dominio para el Algoritmo de Rotación de Sábados (RF08 - RF11).
/// Aplica las reglas del 50% de dotación, alternancia quincenal y distribución equitativa de turnos.
/// </summary>
public interface ISabadoRotacionDomainService
{
    /// <summary>
    /// Genera la entidad PlanificacionSabado con sus asignaciones calculadas según las reglas de negocio.
    /// </summary>
    PlanificacionSabado GenerarPropuestaPreliminar(
        DateOnly fechaSabado,
        int idUsuarioCreacion,
        IReadOnlyList<Empleado> empleadosIncorporadosActivos,
        IReadOnlyList<Turno> turnosDisponibles,
        IReadOnlyDictionary<int, DateOnly> ultimoSabadoTrabajadoPorEmpleado,
        IReadOnlyDictionary<int, int> ultimoTurnoSabadoPorEmpleado,
        string? observaciones = null);
}

