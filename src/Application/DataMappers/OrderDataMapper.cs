using FoodMesh.Domain.ValueObjects;

namespace FoodMesh.Application.DataMappers;

/// <summary>
/// Provides static mapping functions between Application DTOs and Domain models.
/// </summary>
public static class OrderDataMapper
{
    public static DeliveryAddress ToDomain(this DeliveryAddressDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new DeliveryAddress(
            street: dto.Street,
            city: dto.City,
            postalCode: dto.PostalCode,
            contactPhoneNumber: dto.ContactPhoneNumber,
            latitude: dto.Latitude,
            longitude: dto.Longitude);
    }

    public static DeliveryAddressDto ToDto(this DeliveryAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);

        return new DeliveryAddressDto
        {
            Street = address.Street,
            City = address.City,
            PostalCode = address.PostalCode,
            ContactPhoneNumber = address.ContactPhoneNumber,
            Latitude = address.Latitude,
            Longitude = address.Longitude
        };
    }

    /// <summary>
    /// Translates the application-level CreateOrderCommand into the pure Domain-level OrderCreationDto.
    /// Acts as the Translator between Application API language and Domain language.
    /// </summary>
    public static FoodMesh.Domain.Models.OrderCreationDto ToDomainDto(
        this FoodMesh.Application.Commands.CreateOrderCommand command,
        Money deliveryFee)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(deliveryFee);

        var address = command.DeliveryAddress.ToDomain();
        var items = command.Items
            .Select(i => new FoodMesh.Domain.Models.OrderItemCreationDto(
                i.MenuItemId,
                i.ItemName,
                new Money(i.UnitPrice, command.Currency),
                i.Quantity))
            .ToList();

        var orderId = command.OrderId != Guid.Empty ? command.OrderId : Guid.NewGuid();

        return new FoodMesh.Domain.Models.OrderCreationDto(
            OrderId: orderId,
            CustomerId: command.CustomerId,
            RestaurantId: command.RestaurantId,
            DeliveryAddress: address,
            DeliveryFee: deliveryFee,
            Items: items);
    }
}
