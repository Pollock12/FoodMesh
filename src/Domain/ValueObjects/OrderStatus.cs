namespace FoodMesh.Domain.ValueObjects;

/// <summary>
/// Represents the life-cycle status of an Order.
/// </summary>
public enum OrderStatus
{
    PendingPayment = 1,
    Paid = 2,
    Preparing = 3,
    ReadyForPickup = 4,
    OutForDelivery = 5,
    Delivered = 6,
    Cancelled = 7
}
