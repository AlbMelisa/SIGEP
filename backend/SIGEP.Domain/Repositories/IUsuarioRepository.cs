using SIGEP.Domain.Common;
using SIGEP.Domain.Entities;

namespace SIGEP.Domain.Repositories;

/// <summary>
/// Contrato del Repositorio de Usuarios.
/// Define las operaciones de persistencia y consulta específicas de la entidad Usuario.
/// </summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByTelegramChatIdAsync(string chatId, CancellationToken cancellationToken = default);
    Task<bool> ExistsUsernameAsync(string username, CancellationToken cancellationToken = default);
}

