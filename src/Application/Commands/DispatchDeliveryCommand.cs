using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.Commands;

/// <summary>
/// Command sent when the restaurant owner departs to deliver the food to the customer.
/// </summary>
public sealed record DispatchDeliveryCommand(Guid OrderId) : IRequest<Result<bool>>;
