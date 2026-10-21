namespace SIGEP.Domain.Enums;

/// <summary>
/// Estados posibles de una planificación de sábado (RF08, RF12, RF13).
/// </summary>
public enum EstadoPlanificacion
{
    /// <summary>
    /// Propuesta preliminar generada por el algoritmo, sujeta a modificaciones (RF08, RF12).
    /// </summary>
    Borrador = 1,

    /// <summary>
    /// Planificación confirmada formalmente por Encargado o Jefe. Bloqueada para edición (RF13).
    /// </summary>
    Confirmada = 2,

    /// <summary>
    /// Planificación dada de baja o cancelada por causa de fuerza mayor.
    /// </summary>
    Cancelada = 3
}

