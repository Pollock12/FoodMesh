using FoodMesh.Application.Commands;
using FoodMesh.Application.CommandServices;
using FoodMesh.Application.DataMappers;
using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.Services;
using FoodMesh.Infrastructure;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.CommandHandlers;

/// <summary>
/// Command handler orchestrating the creation of a new Order.
/// Acts as the "Project Manager":
/// 1. Asks CommandService (the Database Researcher) to validate prerequisites.
/// 2. Uses DataMapper (the Translator) to convert Command to DomainDto.
/// 3. Calls Order aggregate to execute pure domain rules.
/// 4. Persists the aggregate in MongoDB via UnitOfWork.
/// </summary>
public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IMongoUnitOfWork _unitOfWork;
    private readonly IOrderCommandService _orderCommandService;
    private readonly IDeliveryFeeCalculator _feeCalculator;
    private readonly IPublisher _publisher;

    public CreateOrderCommandHandler(
        IMongoUnitOfWork unitOfWork,
        IOrderCommandService orderCommandService,
        IDeliveryFeeCalculator feeCalculator,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderCommandService = orderCommandService ?? throw new ArgumentNullException(nameof(orderCommandService));
        _feeCalculator = feeCalculator ?? throw new ArgumentNullException(nameof(feeCalculator));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.Items == null || command.Items.Count == 0)
            return Result<Guid>.Failure("Order must contain at least one item.");

        try
        {
            // 1. Business checks delegated to CommandService (the Database Researcher)
            var isRestaurantActive = await _orderCommandService.IsRestaurantActiveAsync(command.RestaurantId, cancellationToken);
            if (!isRestaurantActive)
                return Result<Guid>.Failure($"Restaurant '{command.RestaurantId}' is currently inactive or does not exist.");

            var areItemsAvailable = await _orderCommandService.AreMenuItemsAvailableAsync(
                command.RestaurantId,
                command.Items.Select(i => i.MenuItemId),
                cancellationToken);

            if (!areItemsAvailable)
                return Result<Guid>.Failure("One or more selected menu items are currently unavailable.");

            // 2. Calculate dynamic delivery fee
            var address = command.DeliveryAddress.ToDomain();
            var deliveryFee = _feeCalculator.CalculateFee(
                address,
                command.RestaurantLatitude,
                command.RestaurantLongitude,
                command.Currency);

            // 3. Translate Command -> DomainDto via DataMapper (the Translator)
            var domainDto = command.ToDomainDto(deliveryFee);

            // 4. Call Domain Aggregate (pure domain business logic)
            var order = Order.Create(domainDto);

            // 5. Persist aggregate to MongoDB
            var orderRepo = _unitOfWork.GetRepository<Order>();
            await orderRepo.InsertAsync(order, cancellationToken);

            // 6. Dispatch domain events
            foreach (var domainEvent in order.DomainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
            order.ClearDomainEvents();

            return Result<Guid>.Success(order.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to create order: {ex.Message}");
        }
    }
}
