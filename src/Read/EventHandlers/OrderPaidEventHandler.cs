using FoodMesh.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FoodMesh.Read.EventHandlers;

/// <summary>
/// Handles OrderPaidDomainEvent to sync read models when payment completes.
/// </summary>
public sealed class OrderPaidEventHandler : INotificationHandler<OrderPaidDomainEvent>
{
    private readonly ILogger<OrderPaidEventHandler> _logger;

    public OrderPaidEventHandler(ILogger<OrderPaidEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(OrderPaidDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[ReadProjection] Order paid: OrderId={OrderId}, Txn={Txn}, Amount={Amount}",
            notification.OrderId,
            notification.TransactionId,
            notification.AmountPaid);

        return Task.CompletedTask;
    }
}
