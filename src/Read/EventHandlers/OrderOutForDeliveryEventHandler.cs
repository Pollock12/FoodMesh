using FoodMesh.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FoodMesh.Read.EventHandlers;

/// <summary>
/// Handles OrderOutForDeliveryDomainEvent to update live tracking read projections.
/// </summary>
public sealed class OrderOutForDeliveryEventHandler : INotificationHandler<OrderOutForDeliveryDomainEvent>
{
    private readonly ILogger<OrderOutForDeliveryEventHandler> _logger;

    public OrderOutForDeliveryEventHandler(ILogger<OrderOutForDeliveryEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(OrderOutForDeliveryDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[ReadProjection] Order out for delivery: OrderId={OrderId}, RiderId={RiderId}",
            notification.OrderId,
            notification.DeliveryPartnerId);

        return Task.CompletedTask;
    }
}
