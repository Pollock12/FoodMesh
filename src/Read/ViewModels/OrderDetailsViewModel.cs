namespace FoodMesh.Read.ViewModels;

/// <summary>
/// Read-optimized ViewModel containing complete information for an Order detail view.
/// </summary>
public sealed class OrderDetailsViewModel
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentTransactionId { get; set; }
    public Guid? AssignedDeliveryPartnerId { get; set; }
    public string? DeliveryPartnerName { get; set; }

    public string DeliveryStreet { get; set; } = string.Empty;
    public string DeliveryCity { get; set; } = string.Empty;
    public string DeliveryPostalCode { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";

    public List<OrderItemDetailsViewModel> Items { get; set; } = [];

    public DateTime PlacedAtUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public string? CancellationReason { get; set; }
}
