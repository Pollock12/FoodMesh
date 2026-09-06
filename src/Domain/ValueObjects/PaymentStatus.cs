namespace FoodMesh.Domain.ValueObjects;

/// <summary>
/// Represents the payment state of an order.
/// </summary>
public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}
