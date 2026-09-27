using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Events;

/// <summary>
/// Domain event published when the restaurant owner/chef starts cooking the food.
/// </summary>
public sealed record OrderCookingStartedDomainEvent(
    Guid OrderId,
    DateTime CookingStartedAtUtc) : IDomainEvent;
