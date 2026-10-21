using System.Linq.Expressions;

namespace SIGEP.Domain.Common;

/// <summary>
/// Contrato genérico de Repositorio orientado al Dominio.
/// </summary>
public interface IRepository<TEntity> where TEntity : Entity
{
    IUnitOfWork UnitOfWork { get; }

    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}

