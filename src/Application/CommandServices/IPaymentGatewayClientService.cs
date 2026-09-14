using FoodMesh.Domain.ValueObjects;

namespace FoodMesh.Application.CommandServices;

public sealed record PaymentGatewayResult(
    bool IsSuccess,
    string? TransactionId,
    string? ErrorMessage);

/// <summary>
/// Service abstraction for interacting with external payment gateways.
/// </summary>
public interface IPaymentGatewayClientService
{
    Task<PaymentGatewayResult> ChargeAsync(
        Guid orderId,
        Money amount,
        string paymentMethod,
        CancellationToken cancellationToken = default);

    Task<PaymentGatewayResult> RefundAsync(
        string transactionId,
        Money amount,
        string reason,
        CancellationToken cancellationToken = default);
}
