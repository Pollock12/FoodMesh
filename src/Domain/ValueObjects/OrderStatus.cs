namespace FoodMesh.Domain.ValueObjects;

/// <summary>
/// Represents the life-cycle status of an Order in the restaurant.
/// </summary>
public enum OrderStatus
{
    PendingPayment = 1,
    Paid = 2,
    Cooking = 3,
    OutForDelivery = 4,
    Delivered = 5,
    Cancelled = 6
}
