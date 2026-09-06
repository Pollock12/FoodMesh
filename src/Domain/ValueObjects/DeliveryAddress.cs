using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.ValueObjects;

/// <summary>
/// Immutable Value Object representing a delivery location and recipient contact details.
/// </summary>
public sealed class DeliveryAddress : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string ContactPhoneNumber { get; }
    public double Latitude { get; }
    public double Longitude { get; }

    public DeliveryAddress(
        string street,
        string city,
        string postalCode,
        string contactPhoneNumber,
        double latitude = 0.0,
        double longitude = 0.0)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Street address is required.", "INVALID_STREET");

        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City is required.", "INVALID_CITY");

        if (string.IsNullOrWhiteSpace(contactPhoneNumber))
            throw new DomainException("Contact phone number is required for delivery.", "INVALID_PHONE");

        Street = street.Trim();
        City = city.Trim();
        PostalCode = postalCode?.Trim() ?? string.Empty;
        ContactPhoneNumber = contactPhoneNumber.Trim();
        Latitude = latitude;
        Longitude = longitude;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street.ToUpperInvariant();
        yield return City.ToUpperInvariant();
        yield return PostalCode.ToUpperInvariant();
        yield return ContactPhoneNumber;
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"{Street}, {City} {PostalCode} (Contact: {ContactPhoneNumber})";
}
