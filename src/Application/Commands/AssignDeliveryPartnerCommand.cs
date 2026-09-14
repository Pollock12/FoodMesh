using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.Commands;

/// <summary>
/// Command to find and assign the nearest available delivery partner to an order.
/// </summary>
public sealed record AssignDeliveryPartnerCommand(
    Guid OrderId) : IRequest<Result<Guid>>;
