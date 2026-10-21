using SIGEP.Domain.Common;

namespace SIGEP.Domain.Entities;

/// <summary>
/// Representa el área o función operativa del empleado (ej: "Servicios", "Tránsito", "Administración", "Cultura").
/// </summary>
public class Funcion : Entity
{
    public string Nombre { get; private set; } = null!;
    public string? Descripcion { get; private set; }

    protected Funcion() { }

    public Funcion(string nombre, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la función no puede estar vacío.", nameof(nombre));

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim();
    }
}

