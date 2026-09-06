using FoodMesh.Domain.ValueObjects;
using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Events;

public sealed record OrderPaidDomainEvent(
    Guid OrderId,
    string TransactionId,
    Money AmountPaid,
    DateTime PaidAtUtc) : IDomainEvent;
