using FoodMesh.Shared.Common;
using MongoDB.Driver;

namespace FoodMesh.Infrastructure;

/// <summary>
/// Unit of Work pattern interface for managing MongoDB transactions across multiple repositories.
/// Ensures atomicity and consistency for complex multi-aggregate business operations.
/// </summary>
public interface IMongoUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The currently active MongoDB client session handle, or null if no transaction has started.
    /// </summary>
    IClientSessionHandle? CurrentSession { get; }

    /// <summary>
    /// Indicates whether an active transaction is currently in progress.
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Direct access to the configured MongoDB database.
    /// </summary>
    IMongoDatabase Database { get; }

    /// <summary>
    /// Begins a new transaction using a new MongoDB client session.
    /// </summary>
    Task<IClientSessionHandle> StartTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the active transaction and releases the session.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Aborts (rolls back) the active transaction and releases the session.
    /// </summary>
    Task AbortTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtains a transactional repository instance for the given entity type.
    /// </summary>
    ITransactionalRepository<TEntity> GetRepository<TEntity>() where TEntity : Entity<Guid>;

    /// <summary>
    /// Convenience helper to execute an action within an automated transaction block.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<IClientSessionHandle, Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience helper to execute a function returning a result within an automated transaction block.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<IClientSessionHandle, Task<TResult>> action, CancellationToken cancellationToken = default);
}
