using FoodMesh.Domain.ValueObjects;
using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Events;

public sealed record OrderPlacedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId,
    Money TotalAmount,
    DateTime PlacedAtUtc) : IDomainEvent;
