using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.Commands;

/// <summary>
/// Command to cancel an order and release any assigned rider.
/// </summary>
public sealed record CancelOrderCommand(
    Guid OrderId,
    string Reason) : IRequest<Result<bool>>;
