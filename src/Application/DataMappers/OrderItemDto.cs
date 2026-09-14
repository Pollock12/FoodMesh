namespace FoodMesh.Application.DataMappers;

/// <summary>
/// Data Transfer Object representing an individual line item in a creation command.
/// </summary>
public sealed class OrderItemDto
{
    public Guid MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
