namespace SIGEP.Domain.Common;

/// <summary>
/// Contrato de Unidad de Trabajo para transacciones atómicas.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

