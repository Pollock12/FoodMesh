using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Events;

public sealed record DeliveryPartnerAssignedDomainEvent(
    Guid OrderId,
    Guid DeliveryPartnerId,
    DateTime AssignedAtUtc) : IDomainEvent;
