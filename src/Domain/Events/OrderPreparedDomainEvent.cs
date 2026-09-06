using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Events;

public sealed record OrderPreparedDomainEvent(
    Guid OrderId,
    DateTime PreparedAtUtc) : IDomainEvent;
