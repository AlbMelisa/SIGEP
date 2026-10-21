using SIGEP.Domain.Common;
using SIGEP.Domain.Enums;

namespace SIGEP.Domain.Entities;

/// <summary>
/// Representa la asignación individual de un agente municipal a un turno de evento especial (RF16-RF18).
/// </summary>
public class AsignacionEvento : Entity
{
    public int IdTurnoEvento { get; private set; }
    public virtual TurnoEvento TurnoEvento { get; private set; } = null!;

    public int IdEmpleado { get; private set; }
    public virtual Empleado Empleado { get; private set; } = null!;

    /// <summary>
    /// Puntaje total ponderado obtenido por el algoritmo de scoring (RF16).
    /// </summary>
    public decimal PuntajeCalculado { get; private set; }

    public EstadoAsignacionEvento Estado { get; private set; }

    /// <summary>
    /// Indica si fue incorporado o reasignado manualmente por el Encargado/Jefe (RF18).
    /// </summary>
    public bool EsAsignacionManual { get; private set; }

    /// <summary>
    /// Justificación en caso de ajuste manual.
    /// </summary>
    public string? MotivoModificacion { get; private set; }

    protected AsignacionEvento() { }

    public AsignacionEvento(
        int idTurnoEvento,
        int idEmpleado,
        decimal puntajeCalculado,
        bool esAsignacionManual = false,
        string? motivoModificacion = null)
    {
        IdTurnoEvento = idTurnoEvento;
        IdEmpleado = idEmpleado;
        PuntajeCalculado = puntajeCalculado;
        Estado = EstadoAsignacionEvento.Propuesto;
        EsAsignacionManual = esAsignacionManual;
        MotivoModificacion = motivoModificacion?.Trim();
    }

    public void Confirmar()
    {
        Estado = EstadoAsignacionEvento.Confirmado;
    }

    public void Reemplazar(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Debe especificarse el motivo del reemplazo.", nameof(motivo));

        Estado = EstadoAsignacionEvento.Reemplazado;
        MotivoModificacion = motivo.Trim();
    }

    public void Cancelar(string motivo)
    {
        Estado = EstadoAsignacionEvento.Cancelado;
        MotivoModificacion = motivo.Trim();
    }
}
