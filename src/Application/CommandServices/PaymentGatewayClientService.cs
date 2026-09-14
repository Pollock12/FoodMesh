using FoodMesh.Domain.ValueObjects;

namespace FoodMesh.Application.CommandServices;

/// <summary>
/// Simulated external payment gateway client implementation.
/// </summary>
public sealed class PaymentGatewayClientService : IPaymentGatewayClientService
{
    public Task<PaymentGatewayResult> ChargeAsync(
        Guid orderId,
        Money amount,
        string paymentMethod,
        CancellationToken cancellationToken = default)
    {
        if (amount.Amount <= 0)
        {
            return Task.FromResult(new PaymentGatewayResult(false, null, "Charge amount must be positive."));
        }

        // Simulate successful charge
        var transactionId = $"TXN_{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
        return Task.FromResult(new PaymentGatewayResult(true, transactionId, null));
    }

    public Task<PaymentGatewayResult> RefundAsync(
        string transactionId,
        Money amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var refundId = $"REF_{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
        return Task.FromResult(new PaymentGatewayResult(true, refundId, null));
    }
}
