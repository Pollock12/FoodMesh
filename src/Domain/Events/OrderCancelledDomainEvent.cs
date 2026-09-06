using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Events;

public sealed record OrderCancelledDomainEvent(
    Guid OrderId,
    string Reason,
    DateTime CancelledAtUtc) : IDomainEvent;
