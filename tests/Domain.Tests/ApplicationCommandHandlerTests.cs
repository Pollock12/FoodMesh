using FluentAssertions;
using FoodMesh.Application.CommandHandlers;
using FoodMesh.Application.Commands;
using FoodMesh.Application.CommandServices;
using FoodMesh.Application.CommandWorker;
using FoodMesh.Application.DataMappers;
using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.DomainServices;
using FoodMesh.Domain.Entities;
using FoodMesh.Domain.Services;
using FoodMesh.Domain.ValueObjects;
using FoodMesh.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FoodMesh.Domain.Tests;

public class ApplicationCommandHandlerTests
{
    private readonly Mock<IMongoUnitOfWork> _mockUow = new();
    private readonly Mock<ITransactionalRepository<Order>> _mockOrderRepo = new();
    private readonly Mock<ITransactionalRepository<DeliveryPartner>> _mockPartnerRepo = new();
    private readonly Mock<IOrderCommandService> _mockOrderCommandService = new();
    private readonly Mock<IPublisher> _mockPublisher = new();
    private readonly Mock<IDeliveryFeeCalculator> _mockFeeCalculator = new();
    private readonly Mock<IPaymentGatewayClientService> _mockPaymentGateway = new();
    private readonly Mock<INotificationCommandService> _mockNotificationService = new();
    private readonly IOrderFulfillmentDomainService _fulfillmentService = new OrderFulfillmentDomainService();

    public ApplicationCommandHandlerTests()
    {
        _mockUow.Setup(u => u.GetRepository<Order>()).Returns(_mockOrderRepo.Object);
        _mockUow.Setup(u => u.GetRepository<DeliveryPartner>()).Returns(_mockPartnerRepo.Object);
        _mockUow
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<MongoDB.Driver.IClientSessionHandle, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<MongoDB.Driver.IClientSessionHandle, Task>, CancellationToken>((action, ct) => action(Mock.Of<MongoDB.Driver.IClientSessionHandle>()));

        // Default setup for CommandService researcher queries
        _mockOrderCommandService.Setup(s => s.IsRestaurantActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockOrderCommandService.Setup(s => s.AreMenuItemsAvailableAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task CreateOrderCommandHandler_Should_Validate_Translate_And_Insert_Order()
    {
        // Arrange
        _mockFeeCalculator
            .Setup(f => f.CalculateFee(It.IsAny<DeliveryAddress>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>()))
            .Returns(new Money(2.50m, "USD"));

        var handler = new CreateOrderCommandHandler(
            _mockUow.Object,
            _mockOrderCommandService.Object,
            _mockFeeCalculator.Object,
            _mockPublisher.Object);

        var command = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            RestaurantId: Guid.NewGuid(),
            RestaurantLatitude: 23.81,
            RestaurantLongitude: 90.41,
            DeliveryAddress: new DeliveryAddressDto
            {
                Street = "123 Main St",
                City = "Dhaka",
                ContactPhoneNumber = "+8801700000000"
            },
            Items:
            [
                new OrderItemDto { MenuItemId = Guid.NewGuid(), ItemName = "Burger", UnitPrice = 10.00m, Quantity = 2 }
            ],
            Currency: "USD");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _mockOrderCommandService.Verify(s => s.IsRestaurantActiveAsync(command.RestaurantId, It.IsAny<CancellationToken>()), Times.Once);
        _mockOrderCommandService.Verify(s => s.AreMenuItemsAvailableAsync(command.RestaurantId, It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockOrderRepo.Verify(r => r.InsertAsync(It.Is<Order>(o => o.Id == result.Value), It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisher.Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderCommandHandler_Should_Fail_When_Restaurant_Inactive()
    {
        // Arrange
        _mockOrderCommandService.Setup(s => s.IsRestaurantActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new CreateOrderCommandHandler(
            _mockUow.Object,
            _mockOrderCommandService.Object,
            _mockFeeCalculator.Object,
            _mockPublisher.Object);

        var command = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            RestaurantId: Guid.NewGuid(),
            RestaurantLatitude: 23.81,
            RestaurantLongitude: 90.41,
            DeliveryAddress: new DeliveryAddressDto { Street = "A", City = "B", ContactPhoneNumber = "123" },
            Items: [new OrderItemDto { MenuItemId = Guid.NewGuid(), ItemName = "Burger", UnitPrice = 10m, Quantity = 1 }]);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("inactive or does not exist");
    }

    [Fact]
    public async Task CreateOrderCommandHandler_Should_Fail_When_Items_Empty()
    {
        // Arrange
        var handler = new CreateOrderCommandHandler(
            _mockUow.Object,
            _mockOrderCommandService.Object,
            _mockFeeCalculator.Object,
            _mockPublisher.Object);

        var command = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            RestaurantId: Guid.NewGuid(),
            RestaurantLatitude: 0,
            RestaurantLongitude: 0,
            DeliveryAddress: new DeliveryAddressDto { Street = "A", City = "B", ContactPhoneNumber = "123" },
            Items: []);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("at least one item");
    }

    [Fact]
    public async Task ProcessPaymentCommandHandler_Should_Charge_And_Transition_Order_To_Paid()
    {
        // Arrange
        var address = new DeliveryAddress("Street", "City", "1212", "01700000000");
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), address, new Money(2.00m, "USD"));
        order.AddItem(Guid.NewGuid(), "Pizza", new Money(15.00m, "USD"), 1);

        _mockOrderCommandService.Setup(s => s.GetOrderAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        _mockPaymentGateway
            .Setup(p => p.ChargeAsync(order.Id, order.TotalAmount, "CreditCard", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResult(true, "TXN_SUCCESS_123", null));

        var handler = new ProcessPaymentCommandHandler(
            _mockUow.Object,
            _mockOrderCommandService.Object,
            _mockPaymentGateway.Object,
            _mockNotificationService.Object,
            _mockPublisher.Object);

        var command = new ProcessPaymentCommand(order.Id, "CreditCard");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("TXN_SUCCESS_123");
        order.Status.Should().Be(OrderStatus.Preparing);
        order.PaymentStatus.Should().Be(PaymentStatus.Completed);

        _mockOrderRepo.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _mockNotificationService.Verify(n => n.NotifyCustomerAsync(order.CustomerId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignDeliveryPartnerCommandHandler_Should_Assign_Closest_Rider_And_Save_Atomically()
    {
        // Arrange
        var address = new DeliveryAddress("Street", "City", "1212", "01700000000", 23.81, 90.41);
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), address, new Money(2.00m, "USD"));
        order.AddItem(Guid.NewGuid(), "Pasta", new Money(12.00m, "USD"), 1);
        order.MarkAsPaid("TXN_1", new Money(14.00m, "USD"));
        order.StartPreparation();

        var rider = new DeliveryPartner(Guid.NewGuid(), "Fast Rider", "+8801999999999", "Bike", 23.81, 90.41);

        _mockOrderCommandService.Setup(s => s.GetOrderAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _mockOrderCommandService.Setup(s => s.GetAvailableDeliveryPartnersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DeliveryPartner> { rider });

        var handler = new AssignDeliveryPartnerCommandHandler(
            _mockUow.Object,
            _mockOrderCommandService.Object,
            _fulfillmentService,
            _mockNotificationService.Object,
            _mockPublisher.Object);

        var command = new AssignDeliveryPartnerCommand(order.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(rider.Id);
        order.AssignedDeliveryPartnerId.Should().Be(rider.Id);
        rider.IsAvailable.Should().BeFalse();

        _mockOrderRepo.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _mockPartnerRepo.Verify(r => r.UpdateAsync(rider, It.IsAny<CancellationToken>()), Times.Once);
        _mockNotificationService.Verify(n => n.NotifyRiderAsync(rider.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrderCommandHandler_Should_Cancel_And_Release_Assigned_Partner()
    {
        // Arrange
        var address = new DeliveryAddress("Street", "City", "1212", "01700000000");
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), address, new Money(2.00m, "USD"));
        order.AddItem(Guid.NewGuid(), "Salad", new Money(8.00m, "USD"), 1);
        order.MarkAsPaid("TXN_1", new Money(10.00m, "USD"));
        order.StartPreparation();

        var rider = new DeliveryPartner(Guid.NewGuid(), "Rider", "+123", "Bike");
        _fulfillmentService.AssignBestAvailableRider(order, [rider]);

        _mockOrderCommandService.Setup(s => s.GetOrderAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _mockOrderCommandService.Setup(s => s.GetDeliveryPartnerAsync(rider.Id, It.IsAny<CancellationToken>())).ReturnsAsync(rider);

        var handler = new CancelOrderCommandHandler(
            _mockUow.Object,
            _mockOrderCommandService.Object,
            _fulfillmentService,
            _mockPublisher.Object);

        var command = new CancelOrderCommand(order.Id, "Customer decided to eat outside");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        rider.IsAvailable.Should().BeTrue();
        rider.ActiveOrderId.Should().BeNull();

        _mockOrderRepo.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _mockPartnerRepo.Verify(r => r.UpdateAsync(rider, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CommandConsumerAdapter_Should_Dispatch_Command_To_Mediator()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(IMediator))).Returns(mockMediator.Object);
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeFactory.Object);

        var adapter = new CommandConsumerAdapter<CancelOrderCommand>();
        var command = new CancelOrderCommand(Guid.NewGuid(), "Testing background worker");

        // Act
        await adapter.ConsumeAsync(command, mockServiceProvider.Object);

        // Assert
        mockMediator.Verify(m => m.Send((object)command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
