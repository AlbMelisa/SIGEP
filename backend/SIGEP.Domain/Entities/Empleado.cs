using SIGEP.Domain.Common;
using SIGEP.Domain.Enums;

namespace SIGEP.Domain.Entities;

/// <summary>
/// Entidad de Dominio: Empleado de la Municipalidad de Simoca.
/// Encapsula sus datos personales, laborales y las reglas de turno habitual.
/// </summary>
public class Empleado : Entity
{
    public string Nombre { get; private set; } = null!;
    public string Apellido { get; private set; } = null!;
    public string Documento { get; private set; } = null!;
    public string? Direccion { get; private set; }
    public string? Telefono { get; private set; }
    public string? Email { get; private set; }

    // Función / Área
    public int FuncionId { get; private set; }
    public virtual Funcion Funcion { get; private set; } = null!;

    // Régimen laboral (Incorporado vs. Histórico)
    public int RegimenId { get; private set; }
    public virtual Regimen Regimen { get; private set; } = null!;

    // Turno habitual de referencia para lunes a viernes (RF03)
    public int TurnoHabitualId { get; private set; }
    public virtual Turno TurnoHabitual { get; private set; } = null!;

    public EstadoEmpleado Estado { get; private set; }
    public DateTime FechaIngreso { get; private set; }

    public string NombreCompleto => $"{Nombre} {Apellido}";

    protected Empleado() { }

    public Empleado(
        string nombre,
        string apellido,
        string documento,
        int funcionId,
        int regimenId,
        int turnoHabitualId,
        string? direccion = null,
        string? telefono = null,
        string? email = null,
        DateTime? fechaIngreso = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del empleado no puede estar vacío.", nameof(nombre));

        if (string.IsNullOrWhiteSpace(apellido))
            throw new ArgumentException("El apellido del empleado no puede estar vacío.", nameof(apellido));

        if (string.IsNullOrWhiteSpace(documento))
            throw new ArgumentException("El documento (DNI) es obligatorio.", nameof(documento));

        if (funcionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(funcionId), "Debe especificar una función válida.");

        if (regimenId <= 0)
            throw new ArgumentOutOfRangeException(nameof(regimenId), "Debe especificar un régimen válido.");

        if (turnoHabitualId <= 0)
            throw new ArgumentOutOfRangeException(nameof(turnoHabitualId), "Debe especificar un turno habitual válido.");

        Nombre = nombre.Trim();
        Apellido = apellido.Trim();
        Documento = documento.Trim().Replace(".", ""); // Normaliza el DNI
        FuncionId = funcionId;
        RegimenId = regimenId;
        TurnoHabitualId = turnoHabitualId;
        Direccion = direccion?.Trim();
        Telefono = telefono?.Trim();
        Email = email?.Trim().ToLowerInvariant();
        Estado = EstadoEmpleado.Activo;
        FechaIngreso = fechaIngreso ?? DateTime.UtcNow;
    }

    #region Métodos de Negocio (Comportamiento)

    public bool EstaActivo() => Estado == EstadoEmpleado.Activo;

    public void DarDeBaja()
    {
        Estado = EstadoEmpleado.Inactivo;
    }

    public void Reactivar()
    {
        Estado = EstadoEmpleado.Activo;
    }

    public void ModificarDatosPersonales(
        string nombre,
        string apellido,
        string documento,
        int funcionId,
        string? direccion,
        string? telefono,
        string? email)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
        if (string.IsNullOrWhiteSpace(apellido))
            throw new ArgumentException("El apellido no puede estar vacío.", nameof(apellido));
        if (string.IsNullOrWhiteSpace(documento))
            throw new ArgumentException("El documento no puede estar vacío.", nameof(documento));

        Nombre = nombre.Trim();
        Apellido = apellido.Trim();
        Documento = documento.Trim().Replace(".", "");
        FuncionId = funcionId;
        Direccion = direccion?.Trim();
        Telefono = telefono?.Trim();
        Email = email?.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Cambia el régimen y turno habitual validando la regla RF03.
    /// </summary>
    public void AsignarRegimenYTurno(int nuevoRegimenId, int nuevoTurnoId)
    {
        if (nuevoRegimenId <= 0)
            throw new ArgumentOutOfRangeException(nameof(nuevoRegimenId));
        if (nuevoTurnoId <= 0)
            throw new ArgumentOutOfRangeException(nameof(nuevoTurnoId));

        RegimenId = nuevoRegimenId;
        TurnoHabitualId = nuevoTurnoId;
    }

    #endregion
}

