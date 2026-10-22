using SIGEP.Domain.Common;
using SIGEP.Domain.Enums;

namespace SIGEP.Domain.Entities;

/// <summary>
/// Representa un evento municipal extraordinario (Fiesta del Sulky, festivales, etc. - RF14 a RF18).
/// Raíz de agregado que gobierna sus turnos operativos y nóminas asignadas.
/// </summary>
public class Evento : Entity
{
    public string Nombre { get; private set; } = null!;
    public string? Descripcion { get; private set; }
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }
    public string? Lugar { get; private set; }
    public EstadoEvento Estado { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaConfirmacion { get; private set; }
    public int IdUsuarioCreacion { get; private set; }
    public int? IdUsuarioConfirmacion { get; private set; }

    public virtual Usuario UsuarioCreacion { get; private set; } = null!;
    public virtual Usuario? UsuarioConfirmacion { get; private set; }

    private readonly List<TurnoEvento> _turnos = new();
    public virtual IReadOnlyCollection<TurnoEvento> Turnos => _turnos.AsReadOnly();

    protected Evento() { }

    public Evento(
        string nombre,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        int idUsuarioCreacion,
        string? descripcion = null,
        string? lugar = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del evento es obligatorio.", nameof(nombre));

        if (fechaFin < fechaInicio)
            throw new InvalidOperationException("La fecha de finalización no puede ser anterior a la de inicio.");

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim();
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Lugar = lugar?.Trim();
        Estado = EstadoEvento.Planificado;
        FechaCreacion = DateTime.UtcNow;
        IdUsuarioCreacion = idUsuarioCreacion;
    }

    public TurnoEvento AgregarTurno(string nombreTurno, DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin, int cupoRequerido)
    {
        if (Estado != EstadoEvento.Planificado)
            throw new InvalidOperationException("Solo se pueden agregar turnos a eventos en estado Planificado.");

        if (fecha < FechaInicio || fecha > FechaFin)
            throw new InvalidOperationException($"La fecha del turno {fecha:dd/MM/yyyy} debe estar comprendida dentro de las fechas del evento ({FechaInicio:dd/MM/yyyy} a {FechaFin:dd/MM/yyyy}).");

        var turno = new TurnoEvento(Id, nombreTurno, fecha, horaInicio, horaFin, cupoRequerido);
        _turnos.Add(turno);
        return turno;
    }

    public void Confirmar(int idUsuarioConfirmacion)
    {
        if (Estado != EstadoEvento.Planificado)
            throw new InvalidOperationException("Solo se pueden confirmar eventos en estado Planificado.");

        if (!_turnos.Any())
            throw new InvalidOperationException("El evento debe poseer al menos un turno configurado antes de confirmarse.");

        foreach (var turno in _turnos)
        {
            turno.ConfirmarAsignaciones();
        }

        Estado = EstadoEvento.EnCurso;
        FechaConfirmacion = DateTime.UtcNow;
        IdUsuarioConfirmacion = idUsuarioConfirmacion;
    }

    public void Finalizar()
    {
        if (Estado != EstadoEvento.EnCurso)
            throw new InvalidOperationException("Solo se pueden finalizar eventos que se encuentren En Curso.");

        Estado = EstadoEvento.Finalizado;
    }

    public void Cancelar()
    {
        if (Estado == EstadoEvento.Finalizado)
            throw new InvalidOperationException("No se puede cancelar un evento que ya ha finalizado.");

        Estado = EstadoEvento.Cancelado;
    }
}

