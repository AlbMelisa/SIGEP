using SIGEP.Domain.Common;
using SIGEP.Domain.Enums;

namespace SIGEP.Domain.Entities;

/// <summary>
/// Turno u horario operativo específico dentro de un evento especial (RF15).
/// </summary>
public class TurnoEvento : Entity
{
    public int IdEvento { get; private set; }
    public virtual Evento Evento { get; private set; } = null!;

    public string Nombre { get; private set; } = null!;
    public DateOnly Fecha { get; private set; }
    public TimeOnly HoraInicio { get; private set; }
    public TimeOnly HoraFin { get; private set; }
    public int CupoRequerido { get; private set; }

    private readonly List<AsignacionEvento> _asignaciones = new();
    public virtual IReadOnlyCollection<AsignacionEvento> Asignaciones => _asignaciones.AsReadOnly();

    protected TurnoEvento() { }

    public TurnoEvento(int idEvento, string nombre, DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin, int cupoRequerido)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del turno no puede estar vacío.", nameof(nombre));

        if (cupoRequerido <= 0)
            throw new ArgumentOutOfRangeException(nameof(cupoRequerido), "El cupo requerido debe ser mayor a cero.");

        IdEvento = idEvento;
        Nombre = nombre.Trim();
        Fecha = fecha;
        HoraInicio = horaInicio;
        HoraFin = horaFin;
        CupoRequerido = cupoRequerido;
    }

    public void AgregarAsignacion(int idEmpleado, decimal puntaje, bool esManual = false, string? motivo = null)
    {
        if (_asignaciones.Any(a => a.IdEmpleado == idEmpleado && a.Estado != EstadoAsignacionEvento.Cancelado && a.Estado != EstadoAsignacionEvento.Reemplazado))
            throw new InvalidOperationException($"El empleado {idEmpleado} ya se encuentra asignado a este turno de evento.");

        _asignaciones.Add(new AsignacionEvento(Id, idEmpleado, puntaje, esManual, motivo));
    }

    public void ReemplazarAsignacion(int idEmpleadoSaliente, int idEmpleadoEntrante, decimal puntaje, string motivo)
    {
        var asignacion = _asignaciones.FirstOrDefault(a => a.IdEmpleado == idEmpleadoSaliente && a.Estado == EstadoAsignacionEvento.Propuesto)
            ?? throw new KeyNotFoundException($"El empleado {idEmpleadoSaliente} no posee una asignación activa en este turno.");

        if (_asignaciones.Any(a => a.IdEmpleado == idEmpleadoEntrante && a.Estado != EstadoAsignacionEvento.Cancelado && a.Estado != EstadoAsignacionEvento.Reemplazado))
            throw new InvalidOperationException($"El agente entrante {idEmpleadoEntrante} ya está asignado a este turno.");

        asignacion.Reemplazar(motivo);
        _asignaciones.Add(new AsignacionEvento(Id, idEmpleadoEntrante, puntaje, true, motivo));
    }

    public void ConfirmarAsignaciones()
    {
        foreach (var asignacion in _asignaciones.Where(a => a.Estado == EstadoAsignacionEvento.Propuesto))
        {
            asignacion.Confirmar();
        }
    }
}
