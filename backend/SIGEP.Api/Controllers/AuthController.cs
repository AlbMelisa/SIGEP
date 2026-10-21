using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEP.Application.DTOs.Auth;
using SIGEP.Application.Interfaces;
using System.Security.Claims;

namespace SIGEP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _authAppService;

    public AuthController(IAuthAppService authAppService)
    {
        _authAppService = authAppService ?? throw new ArgumentNullException(nameof(authAppService));
    }

    /// <summary>
    /// Endpoint público de inicio de sesión para operadores de SIGEP.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authAppService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ocurrió un error interno al procesar la solicitud.", detalle = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint protegido que retorna los datos del operador actualmente autenticado.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int usuarioId))
            return Unauthorized(new { error = "Token inválido o sin identificador de usuario." });

        var usuario = await _authAppService.GetCurrentUserAsync(usuarioId, cancellationToken);
        if (usuario == null)
            return NotFound(new { error = "Usuario no encontrado o inactivo." });

        return Ok(usuario);
    }
}

