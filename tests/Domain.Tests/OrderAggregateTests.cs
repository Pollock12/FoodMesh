using FluentAssertions;
using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.Events;
using FoodMesh.Domain.ValueObjects;
using FoodMesh.Shared.Common;
using Xunit;

namespace FoodMesh.Domain.Tests;

public class OrderAggregateTests
{
    private readonly DeliveryAddress _address = new(
        street: "123 Food Street",
        city: "Tech City",
        postalCode: "12345",
        contactPhoneNumber: "+1234567890");

    [Fact]
    public void Order_Create_Should_Initialize_And_Raise_OrderPlacedDomainEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var deliveryFee = new Money(2.50m, "USD");

        // Act
        var order = Order.Create(orderId, customerId, restaurantId, _address, deliveryFee);

        // Assert
        order.Id.Should().Be(orderId);
        order.Status.Should().Be(OrderStatus.PendingPayment);
        order.PaymentStatus.Should().Be(PaymentStatus.Pending);
        order.DeliveryFee.Amount.Should().Be(2.50m);
        order.TotalAmount.Amount.Should().Be(2.50m); // Subtotal (0) + 2.50

        // Domain Event Assertion
        order.DomainEvents.Should().ContainSingle(e => e is OrderPlacedDomainEvent);
        var placedEvent = (OrderPlacedDomainEvent)order.DomainEvents.First();
        placedEvent.OrderId.Should().Be(orderId);
        placedEvent.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public void Order_AddItem_Should_Recalculate_Subtotal_And_Total()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), _address, new Money(3.00m, "USD"));
        var burgerId = Guid.NewGuid();

        // Act
        order.AddItem(burgerId, "Cheese Burger", new Money(10.00m, "USD"), 2);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Subtotal.Amount.Should().Be(20.00m);
        order.TotalAmount.Amount.Should().Be(23.00m); // 20.00 + 3.00
    }

    [Fact]
    public void Order_MarkAsPaid_Should_Transition_To_Paid_And_Raise_OrderPaidDomainEvent()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), _address, new Money(2.00m, "USD"));
        order.AddItem(Guid.NewGuid(), "Pasta", new Money(12.00m, "USD"), 1);

        // Act (Total = 14.00 USD)
        order.MarkAsPaid("TXN_12345", new Money(14.00m, "USD"));

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
        order.PaymentStatus.Should().Be(PaymentStatus.Completed);
        order.PaymentTransactionId.Should().Be("TXN_12345");
        order.PaidAtUtc.Should().NotBeNull();

        order.DomainEvents.Should().Contain(e => e is OrderPaidDomainEvent);
    }

    [Fact]
    public void Order_MarkAsPaid_Should_Throw_When_Underpaid()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), _address, new Money(2.00m, "USD"));
        order.AddItem(Guid.NewGuid(), "Pasta", new Money(12.00m, "USD"), 1);

        // Act (Total is 14.00, paying 10.00)
        Action act = () => order.MarkAsPaid("TXN_FAIL", new Money(10.00m, "USD"));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*less than total amount*");
    }

    [Fact]
    public void Order_Should_Follow_Full_Lifecycle_Successfully()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), _address, new Money(2.00m, "USD"));
        order.AddItem(Guid.NewGuid(), "Pizza", new Money(15.00m, "USD"), 1);
        var riderId = Guid.NewGuid();

        // 1. Pay
        order.MarkAsPaid("TXN_999", new Money(17.00m, "USD"));
        order.Status.Should().Be(OrderStatus.Paid);

        // 2. Prepare
        order.StartPreparation();
        order.Status.Should().Be(OrderStatus.Preparing);

        // 3. Assign Rider
        order.AssignDeliveryPartner(riderId);
        order.AssignedDeliveryPartnerId.Should().Be(riderId);

        // 4. Dispatch
        order.DispatchForDelivery();
        order.Status.Should().Be(OrderStatus.OutForDelivery);

        // 5. Deliver
        order.MarkDelivered();
        order.Status.Should().Be(OrderStatus.Delivered);
        order.DeliveredAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Order_Cannot_Be_Cancelled_After_Delivered()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), _address, new Money(2.00m, "USD"));
        order.AddItem(Guid.NewGuid(), "Pizza", new Money(15.00m, "USD"), 1);
        order.MarkAsPaid("TXN_999", new Money(17.00m, "USD"));
        order.StartPreparation();
        order.AssignDeliveryPartner(Guid.NewGuid());
        order.DispatchForDelivery();
        order.MarkDelivered();

        // Act
        Action act = () => order.Cancel("Changed my mind");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*already been delivered*");
    }
}
