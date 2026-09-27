using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Events;

public sealed record OrderOutForDeliveryDomainEvent(
    Guid OrderId,
    DateTime DispatchedAtUtc) : IDomainEvent;
