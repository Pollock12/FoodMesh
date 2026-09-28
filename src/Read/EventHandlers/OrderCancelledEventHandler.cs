using FoodMesh.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FoodMesh.Read.EventHandlers;

/// <summary>
/// Handles OrderCancelledDomainEvent to update read projections and telemetry when an order is cancelled.
/// </summary>
public sealed class OrderCancelledEventHandler : INotificationHandler<OrderCancelledDomainEvent>
{
    private readonly ILogger<OrderCancelledEventHandler> _logger;

    public OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(OrderCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[ReadProjection] Order cancelled: OrderId={OrderId}, Reason={Reason}, CancelledAtUtc={CancelledAtUtc}",
            notification.OrderId,
            notification.Reason,
            notification.CancelledAtUtc);

        return Task.CompletedTask;
    }
}
