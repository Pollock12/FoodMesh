namespace FoodMesh.Read.ViewModels;

/// <summary>
/// Read-optimized ViewModel representing an available menu item for a restaurant.
/// </summary>
public sealed class MenuItemViewModel
{
    public Guid MenuItemId { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Category { get; set; } = string.Empty;
}
