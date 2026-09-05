namespace FoodMesh.Shared.Common;

/// <summary>
/// Base class for Aggregate Roots in Domain-Driven Design.
/// An Aggregate Root is the primary entry-point entity for an aggregate boundary.
/// It records Domain Events that are dispatched after transactions successfully commit.
/// </summary>
/// <typeparam name="TId">The type of unique identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot() : base() { }

    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Read-only collection of domain events raised by this aggregate.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Records a new domain event within this aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all recorded domain events (called after publishing).
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
