using SIGEP.Domain.Common;

namespace SIGEP.Domain.Entities;

/// <summary>
/// Asignación individual de un agente del Régimen Incorporado a un turno específico de sábado (RF08-RF12).
/// </summary>
public class DetallePlanificacionSabado : Entity
{
    public int IdPlanificacionSabado { get; private set; }
    public virtual PlanificacionSabado PlanificacionSabado { get; private set; } = null!;

    public int IdEmpleado { get; private set; }
    public virtual Empleado Empleado { get; private set; } = null!;

    public int IdTurno { get; private set; }
    public virtual Turno Turno { get; private set; } = null!;

    /// <summary>
    /// Indica si esta asignación fue alterada manualmente por el Encargado/Jefe (RF12).
    /// </summary>
    public bool EsModificacionManual { get; private set; }

    /// <summary>
    /// Justificación obligatoria en caso de modificación manual (RF12).
    /// </summary>
    public string? MotivoModificacion { get; private set; }

    protected DetallePlanificacionSabado() { }

    public DetallePlanificacionSabado(
        int idPlanificacionSabado,
        int idEmpleado,
        int idTurno,
        bool esModificacionManual = false,
        string? motivoModificacion = null)
    {
        IdPlanificacionSabado = idPlanificacionSabado;
        IdEmpleado = idEmpleado;
        IdTurno = idTurno;
        EsModificacionManual = esModificacionManual;
        MotivoModificacion = motivoModificacion?.Trim();
    }

    public void ActualizarTurno(int nuevoIdTurno, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Debe especificarse el motivo justificativo del cambio de turno.", nameof(motivo));

        IdTurno = nuevoIdTurno;
        EsModificacionManual = true;
        MotivoModificacion = motivo.Trim();
    }
}

