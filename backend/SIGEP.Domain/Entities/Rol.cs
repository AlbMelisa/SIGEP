using SIGEP.Domain.Common;

namespace SIGEP.Domain.Entities;

/// <summary>
/// Representa un Rol de usuario en el sistema (ej: "Jefe", "Encargado").
/// Hereda de Entity para contar con identidad única (Id).
/// </summary>
public class Rol : Entity
{
    // Constantes fuertemente tipadas para evitar "strings mágicos"
    public const string RolJefe = "Jefe";
    public const string RolEncargado = "Encargado";

    public string Nombre { get; private set; } = null!;
    public string? Descripcion { get; private set; }

    // Constructor sin parámetros requerido por Entity Framework Core para hidratar la entidad
    protected Rol() { }

    // Constructor de negocio: asegura que la entidad nazca en un estado válido
    public Rol(string nombre, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del rol no puede estar vacío.", nameof(nombre));

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim();
    }
}

