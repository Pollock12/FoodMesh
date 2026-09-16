using FoodMesh.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FoodMesh.Read.EventHandlers;

/// <summary>
/// Handles OrderDeliveredDomainEvent to finalize read models upon successful delivery.
/// </summary>
public sealed class OrderDeliveredEventHandler : INotificationHandler<OrderDeliveredDomainEvent>
{
    private readonly ILogger<OrderDeliveredEventHandler> _logger;

    public OrderDeliveredEventHandler(ILogger<OrderDeliveredEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(OrderDeliveredDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[ReadProjection] Order delivered: OrderId={OrderId}, DeliveredAtUtc={DeliveredAtUtc}",
            notification.OrderId,
            notification.DeliveredAtUtc);

        return Task.CompletedTask;
    }
}
