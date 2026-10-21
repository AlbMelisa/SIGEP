using SIGEP.Domain.Common;
using SIGEP.Domain.Enums;

namespace SIGEP.Domain.Entities;

/// <summary>
/// Entidad de Dominio: Registro de asistencia de un empleado para una jornada laboral específica.
/// Mapea los requerimientos funcionales RF04, RF05, RF06 y RF07.
/// </summary>
public class Asistencia : Entity
{
    public int EmpleadoId { get; private set; }
    public virtual Empleado Empleado { get; private set; } = null!;

    public DateOnly Fecha { get; private set; }
    public TimeOnly? HoraIngreso { get; private set; }
    public TimeOnly? HoraSalida { get; private set; }

    public EstadoAsistencia Estado { get; private set; }
    public TipoJornada TipoJornada { get; private set; }

    public int TurnoId { get; private set; }
    public virtual Turno Turno { get; private set; } = null!;

    public int? EventoId { get; private set; }

    public string? Observacion { get; private set; }

    protected Asistencia() { }

    public Asistencia(
        int empleadoId,
        DateOnly fecha,
        int turnoId,
        EstadoAsistencia estado = EstadoAsistencia.Presente,
        TipoJornada tipoJornada = TipoJornada.Ordinaria,
        TimeOnly? horaIngreso = null,
        TimeOnly? horaSalida = null,
        int? eventoId = null,
        string? observacion = null)
    {
        if (empleadoId <= 0)
            throw new ArgumentOutOfRangeException(nameof(empleadoId), "Debe especificar un empleado válido.");

        if (turnoId <= 0)
            throw new ArgumentOutOfRangeException(nameof(turnoId), "Debe especificar un turno válido.");

        EmpleadoId = empleadoId;
        Fecha = fecha;
        TurnoId = turnoId;
        Estado = estado;
        TipoJornada = tipoJornada;
        HoraIngreso = horaIngreso;
        HoraSalida = horaSalida;
        EventoId = eventoId;
        Observacion = observacion?.Trim();
    }

    #region Métodos de Negocio (Comportamiento)

    public void ActualizarMarcacion(
        TimeOnly? horaIngreso,
        TimeOnly? horaSalida,
        EstadoAsistencia estado,
        string? observacion)
    {
        // Regla: si está ausente, franco o licencia, no puede haber marcación de ingreso a menos que cambie a Presente
        if (estado != EstadoAsistencia.Presente)
        {
            HoraIngreso = null;
            HoraSalida = null;
        }
        else
        {
            HoraIngreso = horaIngreso;
            HoraSalida = horaSalida;
        }

        Estado = estado;
        Observacion = observacion?.Trim();
    }

    public void MarcarIngreso(TimeOnly hora)
    {
        HoraIngreso = hora;
        Estado = EstadoAsistencia.Presente;
    }

    public void MarcarSalida(TimeOnly hora)
    {
        HoraSalida = hora;
    }

    public bool EsPresente() => Estado == EstadoAsistencia.Presente;
    public bool ImpideDisponibilidad() => Estado is EstadoAsistencia.Licencia or EstadoAsistencia.NotaMedica or EstadoAsistencia.Franco or EstadoAsistencia.PermisoEspecial;

    #endregion
}

