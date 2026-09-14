using FoodMesh.Domain.ValueObjects;

namespace FoodMesh.Domain.Models;

/// <summary>
/// Domain-level item specification for creating an order.
/// Decoupled from application commands and HTTP requests.
/// </summary>
public sealed record OrderItemCreationDto(
    Guid MenuItemId,
    string ItemName,
    Money UnitPrice,
    int Quantity);

/// <summary>
/// Domain DTO providing all parameters required to instantiate an Order aggregate.
/// Translates intent from Application Commands into pure Domain language.
/// </summary>
public sealed record OrderCreationDto(
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId,
    DeliveryAddress DeliveryAddress,
    Money DeliveryFee,
    IReadOnlyList<OrderItemCreationDto> Items);
