using SIGEP.Domain.Common;
using SIGEP.Domain.Enums;

namespace SIGEP.Domain.Entities;

/// <summary>
/// Entidad de Dominio: Usuario operador del sistema (Jefe o Encargado).
/// Aplica el patrón de "Modelo Rico" (Rich Domain Model) del libro de Microsoft:
/// sus propiedades están encapsuladas y sus cambios se realizan mediante métodos de negocio.
/// </summary>
public class Usuario : Entity
{
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    
    // Clave foránea y propiedad de navegación a Rol
    public int RolId { get; private set; }
    public virtual Rol Rol { get; private set; } = null!;

    // Vinculación opcional con legajo de empleado (RF23)
    public int? EmpleadoId { get; private set; }

    // Identificador de chat en Telegram para vincular consultas del bot (RF21, RF22)
    public string? TelegramChatId { get; private set; }

    public EstadoUsuario Estado { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? UltimoAcceso { get; private set; }

    // Constructor protegido para Entity Framework Core
    protected Usuario() { }

    // Constructor de Dominio: garantiza invariantes
    public Usuario(string username, string passwordHash, int rolId, int? empleadoId = null, string? telegramChatId = null)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Trim().Length < 3)
            throw new ArgumentException("El nombre de usuario debe tener al menos 3 caracteres.", nameof(username));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de contraseña no puede estar vacío.", nameof(passwordHash));

        if (rolId <= 0)
            throw new ArgumentOutOfRangeException(nameof(rolId), "El RolId debe ser válido.");

        Username = username.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        RolId = rolId;
        EmpleadoId = empleadoId;
        TelegramChatId = telegramChatId?.Trim();
        Estado = EstadoUsuario.Activo;
        FechaCreacion = DateTime.UtcNow;
    }

    #region Métodos de Negocio (Comportamiento)

    public bool EstaActivo() => Estado == EstadoUsuario.Activo;

    public void Activar()
    {
        Estado = EstadoUsuario.Activo;
    }

    public void Desactivar()
    {
        Estado = EstadoUsuario.Inactivo;
    }

    public void RegistrarAcceso()
    {
        UltimoAcceso = DateTime.UtcNow;
    }

    public void CambiarPassword(string nuevoPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoPasswordHash))
            throw new ArgumentException("El nuevo hash de contraseña no puede estar vacío.", nameof(nuevoPasswordHash));

        PasswordHash = nuevoPasswordHash;
    }

    public void AsignarTelegramChatId(string? chatId)
    {
        TelegramChatId = string.IsNullOrWhiteSpace(chatId) ? null : chatId.Trim();
    }

    public void VincularEmpleado(int? empleadoId)
    {
        EmpleadoId = empleadoId;
    }

    public void CambiarRol(int nuevoRolId)
    {
        if (nuevoRolId <= 0)
            throw new ArgumentOutOfRangeException(nameof(nuevoRolId), "El rol especificado es inválido.");

        RolId = nuevoRolId;
    }

    #endregion
}

