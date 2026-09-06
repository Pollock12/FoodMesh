using System.Linq.Expressions;
using FoodMesh.Shared.Common;
using MongoDB.Driver;

namespace FoodMesh.Infrastructure;

/// <summary>
/// Generic repository implementation leveraging MongoDB collections.
/// Automatically executes commands within the active Unit of Work session if one is present.
/// </summary>
/// <typeparam name="TEntity">The entity type deriving from Entity&lt;Guid&gt;.</typeparam>
public class TransactionalRepository<TEntity> : ITransactionalRepository<TEntity> where TEntity : Entity<Guid>
{
    private readonly IMongoCollection<TEntity> _collection;
    private readonly IMongoUnitOfWork _unitOfWork;

    public TransactionalRepository(IMongoDatabase database, IMongoUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        
        var collectionName = GetCollectionName();
        _collection = database.GetCollection<TEntity>(collectionName);
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, id);

        if (_unitOfWork.CurrentSession != null)
        {
            return await _collection.Find(_unitOfWork.CurrentSession, filter)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return await _collection.Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Empty;

        if (_unitOfWork.CurrentSession != null)
        {
            var sessionResult = await _collection.Find(_unitOfWork.CurrentSession, filter)
                .ToListAsync(cancellationToken);
            return sessionResult;
        }

        var result = await _collection.Find(filter)
            .ToListAsync(cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (_unitOfWork.CurrentSession != null)
        {
            var sessionResult = await _collection.Find(_unitOfWork.CurrentSession, predicate)
                .ToListAsync(cancellationToken);
            return sessionResult;
        }

        var result = await _collection.Find(predicate)
            .ToListAsync(cancellationToken);
        return result;
    }

    public async Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (_unitOfWork.CurrentSession != null)
        {
            return await _collection.Find(_unitOfWork.CurrentSession, predicate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return await _collection.Find(predicate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (_unitOfWork.CurrentSession != null)
        {
            await _collection.InsertOneAsync(_unitOfWork.CurrentSession, entity, cancellationToken: cancellationToken);
        }
        else
        {
            await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        }
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id);
        var options = new ReplaceOptions { IsUpsert = false };

        if (_unitOfWork.CurrentSession != null)
        {
            await _collection.ReplaceOneAsync(_unitOfWork.CurrentSession, filter, entity, options, cancellationToken);
        }
        else
        {
            await _collection.ReplaceOneAsync(filter, entity, options, cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, id);

        if (_unitOfWork.CurrentSession != null)
        {
            await _collection.DeleteOneAsync(_unitOfWork.CurrentSession, filter, cancellationToken: cancellationToken);
        }
        else
        {
            await _collection.DeleteOneAsync(filter, cancellationToken: cancellationToken);
        }
    }

    private static string GetCollectionName()
    {
        var typeName = typeof(TEntity).Name;
        // Simple pluralization: Order -> Orders, DeliveryPartner -> DeliveryPartners
        if (typeName.EndsWith('s'))
            return typeName;
        if (typeName.EndsWith('y'))
            return string.Concat(typeName.AsSpan(0, typeName.Length - 1), "ies");

        return $"{typeName}s";
    }
}
