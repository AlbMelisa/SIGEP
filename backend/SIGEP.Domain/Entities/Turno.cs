using SIGEP.Domain.Common;

namespace SIGEP.Domain.Entities;

/// <summary>
/// Representa una franja horaria de trabajo asociada a un régimen laboral específico.
/// </summary>
public class Turno : Entity
{
    public string Nombre { get; private set; } = null!;
    public TimeOnly HoraInicio { get; private set; }
    public TimeOnly HoraFin { get; private set; }
    public string? Descripcion { get; private set; }

    public int RegimenId { get; private set; }
    public virtual Regimen Regimen { get; private set; } = null!;

    protected Turno() { }

    public Turno(string nombre, TimeOnly horaInicio, TimeOnly horaFin, int regimenId, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del turno no puede estar vacío.", nameof(nombre));

        if (regimenId <= 0)
            throw new ArgumentOutOfRangeException(nameof(regimenId), "El turno debe pertenecer a un régimen válido.");

        Nombre = nombre.Trim();
        HoraInicio = horaInicio;
        HoraFin = horaFin;
        RegimenId = regimenId;
        Descripcion = descripcion?.Trim();
    }

    /// <summary>
    /// Indica si el turno es nocturno (cruza la medianoche o abarca horario nocturno).
    /// Crítico para aplicar la regla de descanso de 48 hs del Régimen Histórico.
    /// </summary>
    public bool EsNocturno() => Nombre.Contains("Noche", StringComparison.OrdinalIgnoreCase);
}

