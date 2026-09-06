using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Events;

public sealed record OrderDeliveredDomainEvent(
    Guid OrderId,
    DateTime DeliveredAtUtc) : IDomainEvent;
