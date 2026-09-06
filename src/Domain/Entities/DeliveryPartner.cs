using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Entities;

/// <summary>
/// Entity representing a delivery partner (rider).
/// Manages rider availability, vehicle type, location coordinates, and active delivery assignments.
/// </summary>
public sealed class DeliveryPartner : Entity<Guid>
{
    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string VehicleType { get; private set; } = "Bike";
    public bool IsAvailable { get; private set; }
    public double CurrentLatitude { get; private set; }
    public double CurrentLongitude { get; private set; }
    public Guid? ActiveOrderId { get; private set; }

    private DeliveryPartner() : base() { }

    public DeliveryPartner(
        Guid id,
        string fullName,
        string phoneNumber,
        string vehicleType = "Bike",
        double latitude = 0.0,
        double longitude = 0.0) : base(id)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Delivery partner name is required.", "INVALID_NAME");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Delivery partner phone number is required.", "INVALID_PHONE");

        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
        VehicleType = vehicleType.Trim();
        IsAvailable = true;
        CurrentLatitude = latitude;
        CurrentLongitude = longitude;
        ActiveOrderId = null;
    }

    public void AssignOrder(Guid orderId)
    {
        if (!IsAvailable)
            throw new DomainException($"Delivery partner {FullName} is currently unavailable.", "PARTNER_UNAVAILABLE");

        if (ActiveOrderId.HasValue)
            throw new DomainException($"Delivery partner already has an active order ({ActiveOrderId.Value}).", "PARTNER_BUSY");

        ActiveOrderId = orderId;
        IsAvailable = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void CompleteOrder()
    {
        ActiveOrderId = null;
        IsAvailable = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateLocation(double latitude, double longitude)
    {
        CurrentLatitude = latitude;
        CurrentLongitude = longitude;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetAvailability(bool isAvailable)
    {
        if (!isAvailable && ActiveOrderId.HasValue)
            throw new DomainException("Cannot set status to unavailable while assigned to an active order.", "ACTIVE_ORDER_PRESENT");

        IsAvailable = isAvailable;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
