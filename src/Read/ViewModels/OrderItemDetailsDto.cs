namespace FoodMesh.Read.ViewModels;

/// <summary>
/// Read-optimized ViewModel for an individual order item on the order details screen.
/// </summary>
public sealed class OrderItemDetailsDto
{
    public Guid MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public int Quantity { get; set; }
    public decimal Subtotal => UnitPrice * Quantity;
}
