namespace FoodMesh.Read.ViewModels;

/// <summary>
/// Read-optimized ViewModel representing restaurant metadata and active menu items.
/// </summary>
public sealed class RestaurantSummaryDto
{
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string Cuisine { get; set; } = string.Empty;
    public int AvailableItemCount { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
