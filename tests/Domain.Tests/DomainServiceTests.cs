using FluentAssertions;
using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.DomainServices;
using FoodMesh.Domain.Entities;
using FoodMesh.Domain.ValueObjects;
using Xunit;

namespace FoodMesh.Domain.Tests;

public class DomainServiceTests
{
    private readonly DeliveryAddress _customerAddress = new(
        street: "456 Avenue",
        city: "Dhaka",
        postalCode: "1212",
        contactPhoneNumber: "+8801700000000");

    [Fact]
    public void DeliveryFeeCalculator_Should_Return_Standard_Fee()
    {
        // Arrange
        var calculator = new DeliveryFeeCalculator();

        // Act
        var fee = calculator.CalculateFee(_customerAddress);

        // Assert
        fee.Amount.Should().Be(2.50m);
        fee.Currency.Should().Be("USD");
    }

    [Fact]
    public void OrderFulfillmentDomainService_Should_Assign_Available_Rider()
    {
        // Arrange
        var service = new OrderFulfillmentDomainService();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), _customerAddress, new Money(2.50m, "USD"));
        order.AddItem(Guid.NewGuid(), "Burger", new Money(10.00m, "USD"), 1);
        order.MarkAsPaid("TXN_1", new Money(12.50m, "USD"));
        order.StartPreparation();

        var rider1 = new DeliveryPartner(Guid.NewGuid(), "Rider 1", "111", "Bike");
        var rider2 = new DeliveryPartner(Guid.NewGuid(), "Rider 2", "222", "Bike");

        var riders = new List<DeliveryPartner> { rider1, rider2 };

        // Act
        var result = service.AssignBestAvailableRider(order, riders);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.AssignedPartnerId.Should().Be(rider1.Id);
        order.AssignedDeliveryPartnerId.Should().Be(rider1.Id);
        rider1.IsAvailable.Should().BeFalse();
        rider1.ActiveOrderId.Should().Be(order.Id);
    }

    [Fact]
    public void OrderFulfillmentDomainService_Should_Fail_When_No_Riders_Available()
    {
        // Arrange
        var service = new OrderFulfillmentDomainService();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), _customerAddress, new Money(2.50m, "USD"));

        var riders = new List<DeliveryPartner>(); // Empty pool

        // Act
        var result = service.AssignBestAvailableRider(order, riders);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No available delivery partners");
    }

    [Fact]
    public void OrderFulfillmentDomainService_CancelFulfillment_Should_Release_Rider()
    {
        // Arrange
        var service = new OrderFulfillmentDomainService();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), _customerAddress, new Money(2.50m, "USD"));
        order.AddItem(Guid.NewGuid(), "Burger", new Money(10.00m, "USD"), 1);
        order.MarkAsPaid("TXN_1", new Money(12.50m, "USD"));
        order.StartPreparation();

        var rider = new DeliveryPartner(Guid.NewGuid(), "Rider 1", "333", "Bike");
        service.AssignBestAvailableRider(order, [rider]);

        // Act
        service.CancelFulfillment(order, rider, "Customer cancelled before dispatch");

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        rider.IsAvailable.Should().BeTrue();
        rider.ActiveOrderId.Should().BeNull();
    }
}
