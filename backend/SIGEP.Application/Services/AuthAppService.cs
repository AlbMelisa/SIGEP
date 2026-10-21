using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SIGEP.Application.DTOs.Auth;
using SIGEP.Application.Interfaces;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SIGEP.Application.Services;

/// <summary>
/// Servicio de Aplicación de Autenticación.
/// Coordina la lógica de inicio de sesión, verificación criptográfica y emisión de tokens JWT.
/// Sigue la regla del libro de Microsoft: no contiene reglas de dominio, solo orquesta.
/// </summary>
public class AuthAppService : IAuthAppService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;

    public AuthAppService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Debe proporcionar nombre de usuario y contraseña.");

        // 1. Consulta al Repositorio de Dominio
        var usuario = await _usuarioRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (usuario == null)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        // 2. Consulta regla de negocio en la propia entidad
        if (!usuario.EstaActivo())
            throw new UnauthorizedAccessException("El usuario se encuentra inactivo. Comuníquese con la administración.");

        // 3. Verificación criptográfica de contraseña
        bool passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
        if (!passwordValida)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        // 4. Invocación de comportamiento en la entidad de negocio
        usuario.RegistrarAcceso();

        // 5. Persistencia mediante Unidad de Trabajo (Unit of Work)
        await _usuarioRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Generación del Token JWT
        string token = GenerarTokenJwt(usuario);

        // 7. Mapeo a DTO de salida
        var usuarioDto = new UsuarioDto(
            Id: usuario.Id,
            Username: usuario.Username,
            Rol: usuario.Rol?.Nombre ?? Rol.RolEncargado,
            EmpleadoId: usuario.EmpleadoId,
            TelegramChatId: usuario.TelegramChatId,
            Estado: usuario.Estado.ToString(),
            UltimoAcceso: usuario.UltimoAcceso
        );

        return new LoginResponseDto(token, usuarioDto);
    }

    public async Task<UsuarioDto?> GetCurrentUserAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId, cancellationToken);
        if (usuario == null || !usuario.EstaActivo())
            return null;

        return new UsuarioDto(
            Id: usuario.Id,
            Username: usuario.Username,
            Rol: usuario.Rol?.Nombre ?? Rol.RolEncargado,
            EmpleadoId: usuario.EmpleadoId,
            TelegramChatId: usuario.TelegramChatId,
            Estado: usuario.Estado.ToString(),
            UltimoAcceso: usuario.UltimoAcceso
        );
    }

    private string GenerarTokenJwt(Usuario usuario)
    {
        var secret = _configuration["Jwt:Secret"] ?? "super_secreto_sigep_simoca_2026_jwt_key_minimo_32_caracteres!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Username),
            new(ClaimTypes.Role, usuario.Rol?.Nombre ?? Rol.RolEncargado),
        };

        if (usuario.EmpleadoId.HasValue)
        {
            claims.Add(new Claim("empleado_id", usuario.EmpleadoId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "SIGEP.Api",
            audience: _configuration["Jwt:Audience"] ?? "SIGEP.Client",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

