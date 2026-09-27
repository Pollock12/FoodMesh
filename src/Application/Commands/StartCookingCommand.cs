using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.Commands;

/// <summary>
/// Command sent when the chef begins cooking the order in the kitchen.
/// </summary>
public sealed record StartCookingCommand(Guid OrderId) : IRequest<Result<bool>>;
