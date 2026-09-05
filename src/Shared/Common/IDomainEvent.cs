using MediatR;

namespace FoodMesh.Shared.Common;

/// <summary>
/// Marker interface representing a business event that occurred in the domain.
/// Extends MediatR's INotification so it can be dispatched to in-process event handlers.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Unique identifier for this specific event occurrence.
    /// </summary>
    Guid EventId => Guid.NewGuid();

    /// <summary>
    /// UTC timestamp when the domain event occurred.
    /// </summary>
    DateTime OccurredOn => DateTime.UtcNow;
}
