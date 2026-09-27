using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.Commands;

/// <summary>
/// Command sent when the food is delivered to the customer at their doorstep.
/// </summary>
public sealed record CompleteDeliveryCommand(Guid OrderId) : IRequest<Result<bool>>;
