using FoodMesh.Domain.Services;
using FoodMesh.Domain.ValueObjects;

namespace FoodMesh.Domain.DomainServices;

/// <summary>
/// Domain service implementing dynamic delivery fee calculations based on geographic distance.
/// </summary>
public sealed class DeliveryFeeCalculator : IDeliveryFeeCalculator
{
    private const decimal BaseFee = 2.00m;
    private const decimal RatePerKm = 0.50m;
    private const double BaseDistanceKm = 2.0;

    public Money CalculateFee(
        DeliveryAddress customerAddress,
        double restaurantLatitude,
        double restaurantLongitude,
        string currency = "USD")
    {
        // If coordinates are not provided (0.0), return standard base fee
        if (customerAddress.Latitude == 0.0 && customerAddress.Longitude == 0.0)
            return new Money(BaseFee, currency);

        var distanceKm = CalculateHaversineDistance(
            restaurantLatitude,
            restaurantLongitude,
            customerAddress.Latitude,
            customerAddress.Longitude);

        decimal fee = BaseFee;
        if (distanceKm > BaseDistanceKm)
        {
            var extraKm = (decimal)(distanceKm - BaseDistanceKm);
            fee += Math.Round(extraKm * RatePerKm, 2, MidpointRounding.AwayFromZero);
        }

        return new Money(fee, currency);
    }

    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusKm = 6371.0;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * (Math.PI / 180.0);
}
