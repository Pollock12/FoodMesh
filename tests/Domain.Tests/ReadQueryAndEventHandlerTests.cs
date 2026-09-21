using FluentAssertions;
using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.Entities;
using FoodMesh.Domain.Events;
using FoodMesh.Domain.ValueObjects;
using FoodMesh.Read.EventHandlers;
using FoodMesh.Read.QueryHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace FoodMesh.Domain.Tests;

public class ReadQueryAndEventHandlerTests
{
    private readonly Mock<IMongoDatabase> _mockDatabase = new();
    private readonly Mock<IMongoCollection<Order>> _mockOrdersCollection = new();
    private readonly Mock<IMongoCollection<DeliveryPartner>> _mockPartnersCollection = new();

    public ReadQueryAndEventHandlerTests()
    {
        _mockDatabase.Setup(d => d.GetCollection<Order>("Orders", It.IsAny<MongoCollectionSettings>()))
            .Returns(_mockOrdersCollection.Object);
        _mockDatabase.Setup(d => d.GetCollection<DeliveryPartner>("DeliveryPartners", It.IsAny<MongoCollectionSettings>()))
            .Returns(_mockPartnersCollection.Object);
    }

    [Fact]
    public async Task GetOrderByIdQueryHandler_Should_Fail_When_Order_Not_Found()
    {
        // Arrange
        var mockCursor = new Mock<IAsyncCursor<Order>>();
        mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>())).Returns(false);
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        _mockOrdersCollection
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Order>>(),
                It.IsAny<FindOptions<Order, Order>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        var handler = new GetOrderByIdQueryHandler(_mockDatabase.Object);
        var query = new GetOrderByIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }


    [Fact]
    public async Task EventHandlers_Should_Process_Domain_Events_Without_Error()
    {
        // Arrange
        var mockLoggerPlaced = new Mock<ILogger<OrderPlacedEventHandler>>();
        var mockLoggerPaid = new Mock<ILogger<OrderPaidEventHandler>>();
        var mockLoggerOut = new Mock<ILogger<OrderOutForDeliveryEventHandler>>();
        var mockLoggerDelivered = new Mock<ILogger<OrderDeliveredEventHandler>>();

        var placedHandler = new OrderPlacedEventHandler(mockLoggerPlaced.Object);
        var paidHandler = new OrderPaidEventHandler(mockLoggerPaid.Object);
        var outHandler = new OrderOutForDeliveryEventHandler(mockLoggerOut.Object);
        var deliveredHandler = new OrderDeliveredEventHandler(mockLoggerDelivered.Object);

        var orderId = Guid.NewGuid();
        var placedEvent = new OrderPlacedDomainEvent(orderId, Guid.NewGuid(), Guid.NewGuid(), new Money(25m, "USD"), DateTime.UtcNow);
        var paidEvent = new OrderPaidDomainEvent(orderId, "TXN_123", new Money(25m, "USD"), DateTime.UtcNow);
        var outEvent = new OrderOutForDeliveryDomainEvent(orderId, Guid.NewGuid(), DateTime.UtcNow);
        var deliveredEvent = new OrderDeliveredDomainEvent(orderId, DateTime.UtcNow);

        // Act & Assert
        Func<Task> actPlaced = () => placedHandler.Handle(placedEvent, CancellationToken.None);
        Func<Task> actPaid = () => paidHandler.Handle(paidEvent, CancellationToken.None);
        Func<Task> actOut = () => outHandler.Handle(outEvent, CancellationToken.None);
        Func<Task> actDelivered = () => deliveredHandler.Handle(deliveredEvent, CancellationToken.None);

        await actPlaced.Should().NotThrowAsync();
        await actPaid.Should().NotThrowAsync();
        await actOut.Should().NotThrowAsync();
        await actDelivered.Should().NotThrowAsync();
    }
}
