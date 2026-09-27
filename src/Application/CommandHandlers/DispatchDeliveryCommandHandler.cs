using FoodMesh.Application.Commands;
using FoodMesh.Application.CommandServices;
using FoodMesh.Domain.Aggregates;
using FoodMesh.Infrastructure;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.CommandHandlers;

/// <summary>
/// Command handler for when the restaurant owner departs to deliver the food to the customer.
/// In this self-operated model, no external rider is required.
/// </summary>
public sealed class DispatchDeliveryCommandHandler : IRequestHandler<DispatchDeliveryCommand, Result<bool>>
{
    private readonly IMongoUnitOfWork _unitOfWork;
    private readonly IOrderCommandService _orderCommandService;
    private readonly INotificationCommandService _notificationService;
    private readonly IPublisher _publisher;

    public DispatchDeliveryCommandHandler(
        IMongoUnitOfWork unitOfWork,
        IOrderCommandService orderCommandService,
        INotificationCommandService notificationService,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderCommandService = orderCommandService ?? throw new ArgumentNullException(nameof(orderCommandService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task<Result<bool>> Handle(DispatchDeliveryCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderCommandService.GetOrderAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result<bool>.Failure($"Order with ID '{command.OrderId}' not found.");

        try
        {
            // Execute domain logic
            order.DispatchForDelivery();

            // Persist changes
            var orderRepo = _unitOfWork.GetRepository<Order>();
            await orderRepo.UpdateAsync(order, cancellationToken);

            // Publish domain events
            foreach (var domainEvent in order.DomainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
            order.ClearDomainEvents();

            // Continuous notification to the customer
            await _notificationService.NotifyCustomerAsync(
                order.CustomerId,
                "Delivery On The Way",
                "Your order is on the way! The chef is personally delivering your food to your address.",
                cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to dispatch delivery: {ex.Message}");
        }
    }
}
