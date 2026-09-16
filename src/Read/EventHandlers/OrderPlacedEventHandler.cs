using FoodMesh.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FoodMesh.Read.EventHandlers;

/// <summary>
/// Handles OrderPlacedDomainEvent to update read projections and telemetry.
/// </summary>
public sealed class OrderPlacedEventHandler : INotificationHandler<OrderPlacedDomainEvent>
{
    private readonly ILogger<OrderPlacedEventHandler> _logger;

    public OrderPlacedEventHandler(ILogger<OrderPlacedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(OrderPlacedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[ReadProjection] New order placed: OrderId={OrderId}, CustomerId={CustomerId}, Amount={Amount}",
            notification.OrderId,
            notification.CustomerId,
            notification.TotalAmount);

        return Task.CompletedTask;
    }
}
