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
        contactPhoneNumber: "+8801700000000",
        latitude: 23.8103,
        longitude: 90.4125);

    [Fact]
    public void DeliveryFeeCalculator_Should_Return_BaseFee_For_Default_Coordinates()
    {
        // Arrange
        var calculator = new DeliveryFeeCalculator();
        var zeroAddress = new DeliveryAddress("Street", "City", "123", "123", 0.0, 0.0);

        // Act
        var fee = calculator.CalculateFee(zeroAddress, 23.8103, 90.4125);

        // Assert
        fee.Amount.Should().Be(2.00m);
    }

    [Fact]
    public void DeliveryFeeCalculator_Should_Charge_Distance_Surcharge_For_Far_Deliveries()
    {
        // Arrange
        var calculator = new DeliveryFeeCalculator();
        // Distance roughly 10km away
        var farLat = 23.7200;
        var farLon = 90.4125;

        // Act
        var fee = calculator.CalculateFee(_customerAddress, farLat, farLon);

        // Assert (Should exceed 2.00 base fee)
        fee.Amount.Should().BeGreaterThan(2.00m);
    }

    [Fact]
    public void OrderFulfillmentDomainService_Should_Assign_Closest_Available_Rider()
    {
        // Arrange
        var service = new OrderFulfillmentDomainService();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), _customerAddress, new Money(2.00m, "USD"));
        order.AddItem(Guid.NewGuid(), "Burger", new Money(10.00m, "USD"), 1);
        order.MarkAsPaid("TXN_1", new Money(12.00m, "USD"));
        order.StartPreparation();

        var farRider = new DeliveryPartner(Guid.NewGuid(), "Far Rider", "111", "Bike", 23.5000, 90.1000);
        var nearRider = new DeliveryPartner(Guid.NewGuid(), "Near Rider", "222", "Bike", 23.8100, 90.4120);

        var riders = new List<DeliveryPartner> { farRider, nearRider };

        // Act
        var result = service.AssignBestAvailableRider(order, riders);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.AssignedPartnerId.Should().Be(nearRider.Id);
        order.AssignedDeliveryPartnerId.Should().Be(nearRider.Id);
        nearRider.IsAvailable.Should().BeFalse();
        nearRider.ActiveOrderId.Should().Be(order.Id);
    }

    [Fact]
    public void OrderFulfillmentDomainService_Should_Fail_When_No_Riders_Available()
    {
        // Arrange
        var service = new OrderFulfillmentDomainService();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), _customerAddress, new Money(2.00m, "USD"));

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
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), _customerAddress, new Money(2.00m, "USD"));
        order.AddItem(Guid.NewGuid(), "Burger", new Money(10.00m, "USD"), 1);
        order.MarkAsPaid("TXN_1", new Money(12.00m, "USD"));
        order.StartPreparation();

        var rider = new DeliveryPartner(Guid.NewGuid(), "Rider 1", "333", "Bike", 23.8100, 90.4120);
        service.AssignBestAvailableRider(order, [rider]);

        // Act
        service.CancelFulfillment(order, rider, "Customer cancelled before dispatch");

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        rider.IsAvailable.Should().BeTrue();
        rider.ActiveOrderId.Should().BeNull();
    }
}
