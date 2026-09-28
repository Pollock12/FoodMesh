using FoodMesh.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FoodMesh.Read.EventHandlers;

/// <summary>
/// Handles OrderCookingStartedDomainEvent to update read projections and telemetry when the chef begins cooking.
/// </summary>
public sealed class OrderCookingStartedEventHandler : INotificationHandler<OrderCookingStartedDomainEvent>
{
    private readonly ILogger<OrderCookingStartedEventHandler> _logger;

    public OrderCookingStartedEventHandler(ILogger<OrderCookingStartedEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(OrderCookingStartedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[ReadProjection] Cooking started: OrderId={OrderId}, StartedAtUtc={StartedAtUtc}",
            notification.OrderId,
            notification.CookingStartedAtUtc);

        return Task.CompletedTask;
    }
}
