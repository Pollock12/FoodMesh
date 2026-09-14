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
/// Command handler orchestrating delivery partner matching and assignment.
/// Asks CommandService to check order existence and find available riders,
/// delegates matching logic to DomainService, and persists changes atomically.
/// </summary>
public sealed class AssignDeliveryPartnerCommandHandler : IRequestHandler<AssignDeliveryPartnerCommand, Result<Guid>>
{
    private readonly IMongoUnitOfWork _unitOfWork;
    private readonly IOrderCommandService _orderCommandService;
    private readonly IOrderFulfillmentDomainService _fulfillmentService;
    private readonly INotificationCommandService _notificationService;
    private readonly IPublisher _publisher;

    public AssignDeliveryPartnerCommandHandler(
        IMongoUnitOfWork unitOfWork,
        IOrderCommandService orderCommandService,
        IOrderFulfillmentDomainService fulfillmentService,
        INotificationCommandService notificationService,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderCommandService = orderCommandService ?? throw new ArgumentNullException(nameof(orderCommandService));
        _fulfillmentService = fulfillmentService ?? throw new ArgumentNullException(nameof(fulfillmentService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task<Result<Guid>> Handle(AssignDeliveryPartnerCommand command, CancellationToken cancellationToken)
    {
        // 1. Ask CommandService (the Database Researcher) for data
        var order = await _orderCommandService.GetOrderAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result<Guid>.Failure($"Order with ID '{command.OrderId}' not found.");

        var availablePartners = await _orderCommandService.GetAvailableDeliveryPartnersAsync(cancellationToken);

        // 2. Delegate cross-aggregate matching logic to Domain Service
        var fulfillmentResult = _fulfillmentService.AssignBestAvailableRider(order, availablePartners);
        if (!fulfillmentResult.IsSuccess)
            return Result<Guid>.Failure(fulfillmentResult.ErrorMessage!);

        var assignedPartnerId = fulfillmentResult.AssignedPartnerId!.Value;
        var assignedPartner = availablePartners.First(p => p.Id == assignedPartnerId);

        try
        {
            var orderRepo = _unitOfWork.GetRepository<Order>();
            var partnerRepo = _unitOfWork.GetRepository<DeliveryPartner>();

            // 3. Atomically persist changes across both aggregates using MongoDB Unit of Work transaction
            await _unitOfWork.ExecuteInTransactionAsync(async session =>
            {
                await orderRepo.UpdateAsync(order, cancellationToken);
                await partnerRepo.UpdateAsync(assignedPartner, cancellationToken);
            }, cancellationToken);

            // 4. Dispatch domain events
            foreach (var domainEvent in order.DomainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
            order.ClearDomainEvents();

            // 5. Notify Rider
            await _notificationService.NotifyRiderAsync(
                assignedPartnerId,
                "New Delivery Assigned",
                $"You have been assigned to deliver order {order.Id}.",
                cancellationToken);

            return Result<Guid>.Success(assignedPartnerId);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to persist delivery partner assignment: {ex.Message}");
        }
    }
}
