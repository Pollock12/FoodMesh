using FoodMesh.Domain.ValueObjects;

namespace FoodMesh.Domain.Services;

/// <summary>
/// Domain service interface for calculating delivery fees.
/// </summary>
public interface IDeliveryFeeCalculator
{
    Money CalculateFee(DeliveryAddress customerAddress, string currency = "USD");
}
