using FoodMesh.Shared.Common;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace FoodMesh.Infrastructure;

/// <summary>
/// Implementation of the Unit of Work pattern using MongoDB Client Sessions.
/// Enables ACID multi-document transactions across collections.
/// </summary>
public sealed class MongoUnitOfWork : IMongoUnitOfWork
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly IServiceProvider _serviceProvider;
    private IClientSessionHandle? _currentSession;
    private bool _disposed;

    public MongoUnitOfWork(
        IMongoClient client,
        IMongoDatabase database,
        IServiceProvider serviceProvider)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IClientSessionHandle? CurrentSession => _currentSession;

    public bool HasActiveTransaction => _currentSession != null && _currentSession.IsInTransaction;

    public IMongoDatabase Database => _database;

    public async Task<IClientSessionHandle> StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentSession != null)
            throw new InvalidOperationException("A transaction or session is already active in this Unit of Work.");

        _currentSession = await _client.StartSessionAsync(cancellationToken: cancellationToken);
        _currentSession.StartTransaction();
        return _currentSession;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentSession == null || !_currentSession.IsInTransaction)
            throw new InvalidOperationException("No active transaction found to commit.");

        try
        {
            await _currentSession.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            _currentSession.Dispose();
            _currentSession = null;
        }
    }

    public async Task AbortTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentSession != null)
        {
            try
            {
                if (_currentSession.IsInTransaction)
                {
                    await _currentSession.AbortTransactionAsync(cancellationToken);
                }
            }
            finally
            {
                _currentSession.Dispose();
                _currentSession = null;
            }
        }
    }

    public ITransactionalRepository<TEntity> GetRepository<TEntity>() where TEntity : Entity<Guid>
    {
        return _serviceProvider.GetRequiredService<ITransactionalRepository<TEntity>>();
    }

    public async Task ExecuteInTransactionAsync(
        Func<IClientSessionHandle, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        var session = await StartTransactionAsync(cancellationToken);
        try
        {
            await action(session);
            await CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await AbortTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<IClientSessionHandle, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        var session = await StartTransactionAsync(cancellationToken);
        try
        {
            var result = await action(session);
            await CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch
        {
            await AbortTransactionAsync(cancellationToken);
            throw;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _currentSession?.Dispose();
            _currentSession = null;
            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_currentSession != null)
            {
                _currentSession.Dispose();
                _currentSession = null;
            }
            _disposed = true;
            await Task.CompletedTask;
        }
    }
}
