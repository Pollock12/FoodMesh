using FoodMesh.Application.Commands;
using FoodMesh.Application.CommandServices;
using FoodMesh.Domain.Aggregates;
using FoodMesh.Infrastructure;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.CommandHandlers;

/// <summary>
/// Command handler for when the restaurant owner/chef starts cooking an order.
/// Updates the order status to Cooking and dispatches continuous updates to the customer.
/// </summary>
public sealed class StartCookingCommandHandler : IRequestHandler<StartCookingCommand, Result<bool>>
{
    private readonly IMongoUnitOfWork _unitOfWork;
    private readonly IOrderCommandService _orderCommandService;
    private readonly INotificationCommandService _notificationService;
    private readonly IPublisher _publisher;

    public StartCookingCommandHandler(
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

    public async Task<Result<bool>> Handle(StartCookingCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderCommandService.GetOrderAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result<bool>.Failure($"Order with ID '{command.OrderId}' not found.");

        try
        {
            // Execute domain logic
            order.StartCooking();

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
                "Cooking In Progress",
                $"The chef has started cooking your food fresh and hot! 🍳",
                cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to start cooking order: {ex.Message}");
        }
    }
}
