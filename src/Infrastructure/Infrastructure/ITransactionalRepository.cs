using System.Linq.Expressions;
using FoodMesh.Shared.Common;

namespace FoodMesh.Infrastructure;

/// <summary>
/// Generic repository contract that automatically participates in active MongoDB client session transactions.
/// </summary>
/// <typeparam name="TEntity">The entity type deriving from Entity&lt;Guid&gt;.</typeparam>
public interface ITransactionalRepository<TEntity> where TEntity : Entity<Guid>
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
