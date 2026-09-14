using FoodMesh.Application.Commands;
using FoodMesh.Application.CommandServices;
using FoodMesh.Domain.Aggregates;
using FoodMesh.Infrastructure;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.CommandHandlers;

/// <summary>
/// Command handler orchestrating the processing of payment for an order.
/// 1. Asks CommandService to look up the order.
/// 2. Delegates charge to PaymentGatewayClientService.
/// 3. Mutates Order aggregate to Paid/Preparing.
/// 4. Persists changes and notifies customer.
/// </summary>
public sealed class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<string>>
{
    private readonly IMongoUnitOfWork _unitOfWork;
    private readonly IOrderCommandService _orderCommandService;
    private readonly IPaymentGatewayClientService _paymentGateway;
    private readonly INotificationCommandService _notificationService;
    private readonly IPublisher _publisher;

    public ProcessPaymentCommandHandler(
        IMongoUnitOfWork unitOfWork,
        IOrderCommandService orderCommandService,
        IPaymentGatewayClientService paymentGateway,
        INotificationCommandService notificationService,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderCommandService = orderCommandService ?? throw new ArgumentNullException(nameof(orderCommandService));
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task<Result<string>> Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        // 1. Ask CommandService (the Database Researcher) to fetch the order
        var order = await _orderCommandService.GetOrderAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result<string>.Failure($"Order with ID '{command.OrderId}' not found.");

        try
        {
            // 2. Process payment via external gateway
            var paymentResult = await _paymentGateway.ChargeAsync(
                order.Id,
                order.TotalAmount,
                command.PaymentMethod,
                cancellationToken);

            if (!paymentResult.IsSuccess)
                return Result<string>.Failure($"Payment charge failed: {paymentResult.ErrorMessage}");

            var transactionId = paymentResult.TransactionId!;

            // 3. Update Domain Aggregate state
            order.MarkAsPaid(transactionId, order.TotalAmount);
            order.StartPreparation();

            // 4. Save order to MongoDB
            var orderRepo = _unitOfWork.GetRepository<Order>();
            await orderRepo.UpdateAsync(order, cancellationToken);

            // 5. Publish domain events (OrderPaidDomainEvent, OrderPreparedDomainEvent)
            foreach (var domainEvent in order.DomainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
            order.ClearDomainEvents();

            // 6. Notify Customer asynchronously
            await _notificationService.NotifyCustomerAsync(
                order.CustomerId,
                "Payment Confirmed",
                $"Your order {order.Id} has been paid and is now being prepared!",
                cancellationToken);

            return Result<string>.Success(transactionId);
        }
        catch (DomainException ex)
        {
            return Result<string>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Payment processing failed: {ex.Message}");
        }
    }
}
