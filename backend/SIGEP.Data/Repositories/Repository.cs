using Microsoft.EntityFrameworkCore;
using SIGEP.Domain.Common;
using System.Linq.Expressions;

namespace SIGEP.Data.Repositories;

/// <summary>
/// Implementación genérica base del patrón Repositorio usando Entity Framework Core.
/// Implementa IRepository<TEntity> de la capa de Dominio.
/// </summary>
/// <typeparam name="TEntity">Tipo de entidad del dominio.</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    protected readonly SIGEPDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public IUnitOfWork UnitOfWork => Context;

    public Repository(SIGEPDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }
}

