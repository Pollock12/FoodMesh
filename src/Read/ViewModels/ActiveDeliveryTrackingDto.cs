namespace FoodMesh.Read.ViewModels;

/// <summary>
/// Read-optimized ViewModel for live GPS rider tracking and delivery status.
/// </summary>
public sealed class ActiveDeliveryTrackingDto
{
    public Guid OrderId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public Guid? DeliveryPartnerId { get; set; }
    public string? RiderName { get; set; }
    public string? RiderPhone { get; set; }
    public string? VehicleType { get; set; }
    public double RiderLatitude { get; set; }
    public double RiderLongitude { get; set; }
    public double DestinationLatitude { get; set; }
    public double DestinationLongitude { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public DateTime LastUpdatedUtc { get; set; }
}
