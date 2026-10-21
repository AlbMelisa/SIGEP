using SIGEP.Application.DTOs.Auth;

namespace SIGEP.Application.Interfaces;

/// <summary>
/// Contrato del Servicio de Aplicación de Autenticación.
/// Expone los casos de uso disponibles para la capa de presentación (API).
/// </summary>
public interface IAuthAppService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<UsuarioDto?> GetCurrentUserAsync(int usuarioId, CancellationToken cancellationToken = default);
}

