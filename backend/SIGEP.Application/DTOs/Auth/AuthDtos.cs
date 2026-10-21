namespace SIGEP.Application.DTOs.Auth;

public record LoginRequestDto(string Username, string Password);

public record UsuarioDto(
    int Id,
    string Username,
    string Rol,
    int? EmpleadoId,
    string? TelegramChatId,
    string Estado,
    DateTime? UltimoAcceso
);

public record LoginResponseDto(
    string Token,
    UsuarioDto Usuario
);

