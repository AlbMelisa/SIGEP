using Microsoft.EntityFrameworkCore;
using SIGEP.Domain.Entities;
using SIGEP.Domain.Repositories;

namespace SIGEP.Data.Repositories;

/// <summary>
/// Implementación concreta del repositorio de usuarios con Entity Framework Core.
/// </summary>
public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(SIGEPDbContext context) : base(context)
    {
    }

    public override async Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Usuario?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim().ToLowerInvariant();
        return await DbSet
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Username == normalized, cancellationToken);
    }

    public async Task<Usuario?> GetByTelegramChatIdAsync(string chatId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.TelegramChatId == chatId, cancellationToken);
    }

    public async Task<bool> ExistsUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim().ToLowerInvariant();
        return await DbSet.AnyAsync(u => u.Username == normalized, cancellationToken);
    }
}

