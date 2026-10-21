using SIGEP.Domain.Common;

namespace SIGEP.Domain.Entities;

/// <summary>
/// Representa el régimen laboral de un empleado municipal.
/// En Simoca existen dos regímenes: "Incorporado" e "Histórico", cada uno con reglas horarias distintas.
/// </summary>
public class Regimen : Entity
{
    public const string Incorporado = "Incorporado";
    public const string Historico = "Histórico";

    public string Nombre { get; private set; } = null!;
    public string? Descripcion { get; private set; }

    // Colección de turnos asociados a este régimen
    private readonly List<Turno> _turnos = new();
    public virtual IReadOnlyCollection<Turno> Turnos => _turnos.AsReadOnly();

    protected Regimen() { }

    public Regimen(string nombre, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del régimen no puede estar vacío.", nameof(nombre));

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim();
    }
}

