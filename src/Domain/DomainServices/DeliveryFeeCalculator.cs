using FoodMesh.Domain.Services;
using FoodMesh.Domain.ValueObjects;

namespace FoodMesh.Domain.DomainServices;

/// <summary>
/// Domain service implementing standardized delivery fee calculations.
/// </summary>
public sealed class DeliveryFeeCalculator : IDeliveryFeeCalculator
{
    private const decimal StandardDeliveryFee = 2.50m;

    public Money CalculateFee(DeliveryAddress customerAddress, string currency = "USD")
    {
        return new Money(StandardDeliveryFee, currency);
    }
}
