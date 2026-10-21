using SIGEP.Domain.Common;
using SIGEP.Domain.Enums;

namespace SIGEP.Domain.Entities;

/// <summary>
/// Representa la nómina de personal asignada para la cobertura de un día sábado (RF08 - RF13).
/// Agregado raíz que controla la consistencia de las asignaciones y su ciclo de vida.
/// </summary>
public class PlanificacionSabado : Entity
{
    public DateOnly FechaSabado { get; private set; }
    public EstadoPlanificacion Estado { get; private set; }
    public string? Observaciones { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaConfirmacion { get; private set; }
    public int IdUsuarioCreacion { get; private set; }
    public int? IdUsuarioConfirmacion { get; private set; }

    public virtual Usuario UsuarioCreacion { get; private set; } = null!;
    public virtual Usuario? UsuarioConfirmacion { get; private set; }

    private readonly List<DetallePlanificacionSabado> _detalles = new();
    public virtual IReadOnlyCollection<DetallePlanificacionSabado> Detalles => _detalles.AsReadOnly();

    protected PlanificacionSabado() { }

    public PlanificacionSabado(DateOnly fechaSabado, int idUsuarioCreacion, string? observaciones = null)
    {
        if (fechaSabado.DayOfWeek != DayOfWeek.Saturday)
            throw new InvalidOperationException($"La fecha {fechaSabado:dd/MM/yyyy} no corresponde a un día sábado.");

        FechaSabado = fechaSabado;
        Estado = EstadoPlanificacion.Borrador;
        FechaCreacion = DateTime.UtcNow;
        IdUsuarioCreacion = idUsuarioCreacion;
        Observaciones = observaciones?.Trim();
    }

    /// <summary>
    /// Agrega una asignación a la nómina del sábado mientras esté en Borrador (RF08).
    /// </summary>
    public void AgregarAsignacion(int idEmpleado, int idTurno, bool esManual = false, string? motivo = null)
    {
        ValidarModificable();

        if (_detalles.Any(d => d.IdEmpleado == idEmpleado))
            throw new InvalidOperationException($"El empleado {idEmpleado} ya se encuentra asignado a esta planificación.");

        _detalles.Add(new DetallePlanificacionSabado(Id, idEmpleado, idTurno, esManual, motivo));
    }

    /// <summary>
    /// Modifica manualmente el turno de un agente asignado, registrando justificación (RF12).
    /// </summary>
    public void ModificarTurnoEmpleado(int idEmpleado, int nuevoIdTurno, string motivo)
    {
        ValidarModificable();

        var detalle = _detalles.FirstOrDefault(d => d.IdEmpleado == idEmpleado)
            ?? throw new KeyNotFoundException($"El empleado {idEmpleado} no se encuentra asignado a este sábado.");

        detalle.ActualizarTurno(nuevoIdTurno, motivo);
    }

    /// <summary>
    /// Reemplaza un agente asignado por otro agente disponible de dotación, registrando justificación (RF12).
    /// </summary>
    public void ReemplazarEmpleado(int idEmpleadoSaliente, int idEmpleadoEntrante, int idTurno, string motivo)
    {
        ValidarModificable();

        var detalleSaliente = _detalles.FirstOrDefault(d => d.IdEmpleado == idEmpleadoSaliente)
            ?? throw new KeyNotFoundException($"El agente saliente {idEmpleadoSaliente} no se encuentra asignado.");

        if (_detalles.Any(d => d.IdEmpleado == idEmpleadoEntrante))
            throw new InvalidOperationException($"El agente entrante {idEmpleadoEntrante} ya está asignado a este sábado.");

        _detalles.Remove(detalleSaliente);
        _detalles.Add(new DetallePlanificacionSabado(Id, idEmpleadoEntrante, idTurno, true, motivo));
    }

    /// <summary>
    /// Confirma formalmente la nómina del sábado, bloqueando ediciones y habilitando asistencia (RF13).
    /// </summary>
    public void Confirmar(int idUsuarioConfirmacion)
    {
        ValidarModificable();

        if (!_detalles.Any())
            throw new InvalidOperationException("No se puede confirmar una planificación que no posee empleados asignados.");

        Estado = EstadoPlanificacion.Confirmada;
        FechaConfirmacion = DateTime.UtcNow;
        IdUsuarioConfirmacion = idUsuarioConfirmacion;
    }

    /// <summary>
    /// Cancela la planificación en caso de fuerza mayor o anulación justificada.
    /// </summary>
    public void Cancelar(string motivo)
    {
        if (Estado == EstadoPlanificacion.Cancelada)
            throw new InvalidOperationException("La planificación ya se encuentra cancelada.");

        Estado = EstadoPlanificacion.Cancelada;
        Observaciones = string.IsNullOrWhiteSpace(Observaciones)
            ? $"Cancelada: {motivo}"
            : $"{Observaciones} | Cancelada: {motivo}";
    }

    private void ValidarModificable()
    {
        if (Estado != EstadoPlanificacion.Borrador)
            throw new InvalidOperationException($"La planificación no puede ser modificada en estado '{Estado}'.");
    }
}

