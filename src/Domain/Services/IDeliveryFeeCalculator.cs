using FoodMesh.Domain.ValueObjects;

namespace FoodMesh.Domain.Services;

/// <summary>
/// Domain service interface for calculating dynamic delivery fees.
/// </summary>
public interface IDeliveryFeeCalculator
{
    Money CalculateFee(DeliveryAddress customerAddress, double restaurantLatitude, double restaurantLongitude, string currency = "USD");
}
