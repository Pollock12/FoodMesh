using FoodMesh.Application.Commands;
using FoodMesh.Application.CommandServices;
using FoodMesh.Domain.Aggregates;
using FoodMesh.Infrastructure;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.CommandHandlers;

/// <summary>
/// Command handler orchestrating order cancellation for the self-operated restaurant.
/// Enforces domain cancellation invariants directly on the Order aggregate root,
/// persists changes to MongoDB, and dispatches domain events.
/// </summary>
public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<bool>>
{
    private readonly IMongoUnitOfWork _unitOfWork;
    private readonly IOrderCommandService _orderCommandService;
    private readonly IPublisher _publisher;

    public CancelOrderCommandHandler(
        IMongoUnitOfWork unitOfWork,
        IOrderCommandService orderCommandService,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderCommandService = orderCommandService ?? throw new ArgumentNullException(nameof(orderCommandService));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task<Result<bool>> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        // 1. Fetch order
        var order = await _orderCommandService.GetOrderAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result<bool>.Failure($"Order with ID '{command.OrderId}' not found.");

        try
        {
            // 2. Enforce domain logic directly on the Order aggregate root
            order.Cancel(command.Reason);

            // 3. Persist updated order
            var orderRepo = _unitOfWork.GetRepository<Order>();
            await orderRepo.UpdateAsync(order, cancellationToken);

            // 4. Dispatch domain events (e.g. OrderCancelledDomainEvent)
            foreach (var domainEvent in order.DomainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
            order.ClearDomainEvents();

            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to cancel order: {ex.Message}");
        }
    }
}
