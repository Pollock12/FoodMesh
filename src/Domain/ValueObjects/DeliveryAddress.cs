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

    public DeliveryAddress(
        string street,
        string city,
        string postalCode,
        string contactPhoneNumber)
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
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street.ToUpperInvariant();
        yield return City.ToUpperInvariant();
        yield return PostalCode.ToUpperInvariant();
        yield return ContactPhoneNumber;
    }

    public override string ToString() => $"{Street}, {City} {PostalCode} (Contact: {ContactPhoneNumber})";
}
