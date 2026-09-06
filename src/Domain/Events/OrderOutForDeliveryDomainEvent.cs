using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Events;

public sealed record OrderOutForDeliveryDomainEvent(
    Guid OrderId,
    Guid DeliveryPartnerId,
    DateTime DispatchedAtUtc) : IDomainEvent;
