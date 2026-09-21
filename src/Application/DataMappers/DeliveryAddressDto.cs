namespace FoodMesh.Application.DataMappers;

/// <summary>
/// Data Transfer Object representing delivery address parameters in a command.
/// </summary>
public sealed class DeliveryAddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
}
