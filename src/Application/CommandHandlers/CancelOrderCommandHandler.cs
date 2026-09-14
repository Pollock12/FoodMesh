using FoodMesh.Application.Commands;
using FoodMesh.Application.CommandServices;
using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.Entities;
using FoodMesh.Domain.Services;
using FoodMesh.Infrastructure;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.CommandHandlers;

/// <summary>
/// Command handler orchestrating order cancellation and rider release.
/// Asks CommandService to fetch order and partner, delegates cancellation invariants to DomainService,
/// and commits changes in a MongoDB transaction.
/// </summary>
public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<bool>>
{
    private readonly IMongoUnitOfWork _unitOfWork;
    private readonly IOrderCommandService _orderCommandService;
    private readonly IOrderFulfillmentDomainService _fulfillmentService;
    private readonly IPublisher _publisher;

    public CancelOrderCommandHandler(
        IMongoUnitOfWork unitOfWork,
        IOrderCommandService orderCommandService,
        IOrderFulfillmentDomainService fulfillmentService,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderCommandService = orderCommandService ?? throw new ArgumentNullException(nameof(orderCommandService));
        _fulfillmentService = fulfillmentService ?? throw new ArgumentNullException(nameof(fulfillmentService));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task<Result<bool>> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        // 1. Ask CommandService (the Database Researcher) to fetch entities
        var order = await _orderCommandService.GetOrderAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result<bool>.Failure($"Order with ID '{command.OrderId}' not found.");

        DeliveryPartner? assignedPartner = null;
        if (order.AssignedDeliveryPartnerId.HasValue)
        {
            assignedPartner = await _orderCommandService.GetDeliveryPartnerAsync(
                order.AssignedDeliveryPartnerId.Value,
                cancellationToken);
        }

        try
        {
            // 2. Execute domain logic through Domain Service
            _fulfillmentService.CancelFulfillment(order, assignedPartner, command.Reason);

            // 3. Persist atomically in MongoDB transaction
            var orderRepo = _unitOfWork.GetRepository<Order>();
            var partnerRepo = _unitOfWork.GetRepository<DeliveryPartner>();

            await _unitOfWork.ExecuteInTransactionAsync(async session =>
            {
                await orderRepo.UpdateAsync(order, cancellationToken);
                if (assignedPartner != null)
                {
                    await partnerRepo.UpdateAsync(assignedPartner, cancellationToken);
                }
            }, cancellationToken);

            // 4. Dispatch domain events
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
