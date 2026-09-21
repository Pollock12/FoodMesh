using FoodMesh.Application.DataMappers;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.Commands;

/// <summary>
/// Command to place a new food order.
/// </summary>
public sealed record CreateOrderCommand(
    Guid CustomerId,
    Guid RestaurantId,
    DeliveryAddressDto DeliveryAddress,
    List<OrderItemDto> Items,
    string Currency = "USD",
    Guid OrderId = default) : IRequest<Result<Guid>>;
