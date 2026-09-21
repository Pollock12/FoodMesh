using FluentAssertions;
using FoodMesh.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace FoodMesh.Domain.Tests;

public class InfrastructureTests
{
    [Fact]
    public void MongoDbSettings_Should_Have_Sensible_Defaults()
    {
        // Act
        var settings = new MongoDbSettings();

        // Assert
        settings.ConnectionString.Should().Be("mongodb://localhost:27017");
        settings.DatabaseName.Should().Be("FoodMeshDb");
        MongoDbSettings.SectionName.Should().Be("MongoDbSettings");
    }

    [Fact]
    public void BsonClassMaps_Register_Should_Be_Idempotent()
    {
        // Act (Calling Register multiple times must not throw InvalidOperationException)
        Action act1 = () => BsonClassMaps.Register();
        Action act2 = () => BsonClassMaps.Register();

        // Assert
        act1.Should().NotThrow();
        act2.Should().NotThrow();

        // Verify that BsonClassMap was registered for Order
        BsonClassMap.IsClassMapRegistered(typeof(FoodMesh.Domain.Aggregates.Order)).Should().BeTrue();
    }

    [Fact]
    public void BsonSerialization_Should_Serialize_And_Deserialize_Order_With_Only_4_ClassMaps()
    {
        // Ensure registered
        BsonClassMaps.Register();

        // Arrange
        var address = new FoodMesh.Domain.ValueObjects.DeliveryAddress(
            "123 Street", "Dhaka", "1212", "+8801700000000");
        var order = FoodMesh.Domain.Aggregates.Order.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), address, new FoodMesh.Domain.ValueObjects.Money(3.50m, "USD"));
        order.AddItem(Guid.NewGuid(), "Hot Pizza", new FoodMesh.Domain.ValueObjects.Money(15.00m, "USD"), 2);

        // Act - Serialize to BSON Document
        var bsonDoc = order.ToBsonDocument();

        // Assert - DomainEvents must NOT be in the BSON document
        bsonDoc.Contains("DomainEvents").Should().BeFalse();
        bsonDoc.Contains("Items").Should().BeTrue();

        // Act - Deserialize back to Order object
        var deserialized = BsonSerializer.Deserialize<FoodMesh.Domain.Aggregates.Order>(bsonDoc);

        // Assert
        deserialized.Id.Should().Be(order.Id);
        deserialized.CustomerId.Should().Be(order.CustomerId);
        deserialized.Items.Should().HaveCount(1);
        deserialized.Items.First().ItemName.Should().Be("Hot Pizza");
        deserialized.Items.First().Quantity.Should().Be(2);
        deserialized.TotalAmount.Amount.Should().Be(33.50m);
        deserialized.DeliveryAddress.Street.Should().Be("123 Street");
    }

    [Fact]
    public void BsonSerialization_Should_Serialize_DeliveryPartner_And_RestaurantItem_Without_Explicit_ClassMaps()
    {
        // Ensure registered
        BsonClassMaps.Register();

        // Arrange
        var partner = new FoodMesh.Domain.Entities.DeliveryPartner(
            Guid.NewGuid(), "Speedy Rider", "+123456789", "Motorcycle");
        var item = new FoodMesh.Domain.Entities.RestaurantItem(
            Guid.NewGuid(), Guid.NewGuid(), "Burger", "Delicious", new FoodMesh.Domain.ValueObjects.Money(8.50m, "USD"), "FastFood");

        // Act
        var partnerDoc = partner.ToBsonDocument();
        var itemDoc = item.ToBsonDocument();

        var deserializedPartner = BsonSerializer.Deserialize<FoodMesh.Domain.Entities.DeliveryPartner>(partnerDoc);
        var deserializedItem = BsonSerializer.Deserialize<FoodMesh.Domain.Entities.RestaurantItem>(itemDoc);

        // Assert
        deserializedPartner.FullName.Should().Be("Speedy Rider");
        deserializedPartner.VehicleType.Should().Be("Motorcycle");
        deserializedItem.Name.Should().Be("Burger");
        deserializedItem.Price.Amount.Should().Be(8.50m);
    }

    [Fact]
    public async Task MongoUnitOfWork_StartTransaction_Should_Initialize_Session_And_StartTransaction()
    {
        // Arrange
        var mockClient = new Mock<IMongoClient>();
        var mockDatabase = new Mock<IMongoDatabase>();
        var mockSession = new Mock<IClientSessionHandle>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockClient
            .Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSession.Object);

        mockSession.Setup(s => s.IsInTransaction).Returns(true);

        var uow = new MongoUnitOfWork(mockClient.Object, mockDatabase.Object, mockServiceProvider.Object);

        // Act
        var session = await uow.StartTransactionAsync();

        // Assert
        session.Should().NotBeNull();
        uow.CurrentSession.Should().Be(mockSession.Object);
        uow.HasActiveTransaction.Should().BeTrue();
        mockSession.Verify(s => s.StartTransaction(It.IsAny<TransactionOptions>()), Times.Once);
    }

    [Fact]
    public async Task MongoUnitOfWork_CommitTransaction_Should_Commit_And_Clear_Session()
    {
        // Arrange
        var mockClient = new Mock<IMongoClient>();
        var mockDatabase = new Mock<IMongoDatabase>();
        var mockSession = new Mock<IClientSessionHandle>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockClient
            .Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSession.Object);

        mockSession.Setup(s => s.IsInTransaction).Returns(true);

        var uow = new MongoUnitOfWork(mockClient.Object, mockDatabase.Object, mockServiceProvider.Object);
        await uow.StartTransactionAsync();

        // Act
        await uow.CommitTransactionAsync();

        // Assert
        mockSession.Verify(s => s.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockSession.Verify(s => s.Dispose(), Times.Once);
        uow.CurrentSession.Should().BeNull();
        uow.HasActiveTransaction.Should().BeFalse();
    }

    [Fact]
    public async Task MongoUnitOfWork_AbortTransaction_Should_Abort_And_Clear_Session()
    {
        // Arrange
        var mockClient = new Mock<IMongoClient>();
        var mockDatabase = new Mock<IMongoDatabase>();
        var mockSession = new Mock<IClientSessionHandle>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockClient
            .Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSession.Object);

        mockSession.Setup(s => s.IsInTransaction).Returns(true);

        var uow = new MongoUnitOfWork(mockClient.Object, mockDatabase.Object, mockServiceProvider.Object);
        await uow.StartTransactionAsync();

        // Act
        await uow.AbortTransactionAsync();

        // Assert
        mockSession.Verify(s => s.AbortTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockSession.Verify(s => s.Dispose(), Times.Once);
        uow.CurrentSession.Should().BeNull();
        uow.HasActiveTransaction.Should().BeFalse();
    }

    [Fact]
    public async Task MongoUnitOfWork_ExecuteInTransactionAsync_Should_AutoRollback_On_Exception()
    {
        // Arrange
        var mockClient = new Mock<IMongoClient>();
        var mockDatabase = new Mock<IMongoDatabase>();
        var mockSession = new Mock<IClientSessionHandle>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockClient
            .Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSession.Object);

        mockSession.Setup(s => s.IsInTransaction).Returns(true);

        var uow = new MongoUnitOfWork(mockClient.Object, mockDatabase.Object, mockServiceProvider.Object);

        // Act
        Func<Task> act = async () =>
        {
            await uow.ExecuteInTransactionAsync(async session =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Something went wrong during write operations");
            });
        };

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Something went wrong during write operations");

        mockSession.Verify(s => s.AbortTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockSession.Verify(s => s.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        uow.CurrentSession.Should().BeNull();
    }
}
