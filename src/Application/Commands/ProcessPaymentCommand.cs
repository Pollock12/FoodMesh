using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.Commands;

/// <summary>
/// Command to process payment for an existing pending order.
/// </summary>
public sealed record ProcessPaymentCommand(
    Guid OrderId,
    string PaymentMethod = "CreditCard") : IRequest<Result<string>>;
