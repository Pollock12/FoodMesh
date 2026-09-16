namespace FoodMesh.Read.ViewModels;

/// <summary>
/// Lightweight ViewModel for customer order history and listing views.
/// </summary>
public sealed class OrderSummaryDto
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PlacedAtUtc { get; set; }
}
