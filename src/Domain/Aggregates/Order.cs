using FoodMesh.Domain.Entities;
using FoodMesh.Domain.Events;
using FoodMesh.Domain.ValueObjects;
using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Aggregates;

/// <summary>
/// Aggregate Root representing a customer Food Order.
/// Encapsulates OrderItems, DeliveryAddress, pricing invariants, and life-cycle state transitions.
/// </summary>
public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = [];

    public Guid CustomerId { get; private set; }
    public Guid RestaurantId { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; } = default!;
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public string? PaymentTransactionId { get; private set; }
    public Guid? AssignedDeliveryPartnerId { get; private set; }
    public Money DeliveryFee { get; private set; } = default!;
    public string? CancellationReason { get; private set; }

    public DateTime PlacedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Money Subtotal
    {
        get
        {
            if (_items.Count == 0)
                return Money.Zero(DeliveryFee?.Currency ?? "USD");

            var currency = _items[0].UnitPrice.Currency;
            var sum = _items.Sum(i => i.Subtotal.Amount);
            return new Money(sum, currency);
        }
    }

    public Money TotalAmount => Subtotal + DeliveryFee;

    private Order() : base() { }

    private Order(
        Guid id,
        Guid customerId,
        Guid restaurantId,
        DeliveryAddress deliveryAddress,
        Money deliveryFee) : base(id)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID cannot be empty.", "INVALID_CUSTOMER");

        if (restaurantId == Guid.Empty)
            throw new DomainException("Restaurant ID cannot be empty.", "INVALID_RESTAURANT");

        DeliveryAddress = deliveryAddress ?? throw new DomainException("Delivery address is required.", "MISSING_ADDRESS");
        DeliveryFee = deliveryFee ?? Money.Zero();
        CustomerId = customerId;
        RestaurantId = restaurantId;
        Status = OrderStatus.PendingPayment;
        PaymentStatus = PaymentStatus.Pending;
        PlacedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new Order aggregate.
    /// </summary>
    public static Order Create(
        Guid id,
        Guid customerId,
        Guid restaurantId,
        DeliveryAddress deliveryAddress,
        Money deliveryFee)
    {
        var order = new Order(id, customerId, restaurantId, deliveryAddress, deliveryFee);
        order.AddDomainEvent(new OrderPlacedDomainEvent(order.Id, customerId, restaurantId, order.TotalAmount, order.PlacedAtUtc));
        return order;
    }

    /// <summary>
    /// Adds a line item to the order. Can only be modified while order is PendingPayment.
    /// </summary>
    public void AddItem(Guid menuItemId, string itemName, Money unitPrice, int quantity)
    {
        AssertCanModifyOrder();

        var existingItem = _items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var newItem = new OrderItem(Guid.NewGuid(), menuItemId, itemName, unitPrice, quantity);
            _items.Add(newItem);
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a line item from the order.
    /// </summary>
    public void RemoveItem(Guid menuItemId)
    {
        AssertCanModifyOrder();

        var item = _items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (item is null)
            throw new DomainException($"Item with MenuItemId {menuItemId} not found in order.", "ITEM_NOT_FOUND");

        _items.Remove(item);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the order as paid upon successful payment transaction.
    /// </summary>
    public void MarkAsPaid(string transactionId, Money paidAmount)
    {
        if (Status != OrderStatus.PendingPayment)
            throw new DomainException($"Cannot process payment for order in '{Status}' state.", "INVALID_STATE");

        if (_items.Count == 0)
            throw new DomainException("Cannot pay for an empty order without items.", "EMPTY_ORDER");

        if (paidAmount < TotalAmount)
            throw new DomainException($"Paid amount ({paidAmount}) is less than total amount ({TotalAmount}).", "UNDERPAID");

        PaymentStatus = PaymentStatus.Completed;
        PaymentTransactionId = transactionId;
        Status = OrderStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new OrderPaidDomainEvent(Id, transactionId, paidAmount, PaidAtUtc.Value));
    }

    /// <summary>
    /// Updates order status to Preparing when the restaurant begins preparing the food.
    /// </summary>
    public void StartPreparation()
    {
        if (Status != OrderStatus.Paid)
            throw new DomainException("Order must be paid before preparation starts.", "ORDER_NOT_PAID");

        Status = OrderStatus.Preparing;
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new OrderPreparedDomainEvent(Id, DateTime.UtcNow));
    }

    /// <summary>
    /// Assigns a delivery rider to this order.
    /// </summary>
    public void AssignDeliveryPartner(Guid deliveryPartnerId)
    {
        if (deliveryPartnerId == Guid.Empty)
            throw new DomainException("Delivery partner ID cannot be empty.", "INVALID_PARTNER");

        if (Status != OrderStatus.Paid && Status != OrderStatus.Preparing && Status != OrderStatus.ReadyForPickup)
            throw new DomainException($"Cannot assign delivery partner for order in '{Status}' state.", "INVALID_STATE");

        AssignedDeliveryPartnerId = deliveryPartnerId;
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new DeliveryPartnerAssignedDomainEvent(Id, deliveryPartnerId, DateTime.UtcNow));
    }

    /// <summary>
    /// Marks the order as out for delivery once the rider picks it up.
    /// </summary>
    public void DispatchForDelivery()
    {
        if (!AssignedDeliveryPartnerId.HasValue)
            throw new DomainException("Cannot dispatch order without an assigned delivery partner.", "NO_RIDER_ASSIGNED");

        if (Status != OrderStatus.Preparing && Status != OrderStatus.ReadyForPickup && Status != OrderStatus.Paid)
            throw new DomainException($"Cannot dispatch order in '{Status}' state.", "INVALID_STATE");

        Status = OrderStatus.OutForDelivery;
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new OrderOutForDeliveryDomainEvent(Id, AssignedDeliveryPartnerId.Value, DateTime.UtcNow));
    }

    /// <summary>
    /// Marks the order as delivered once the rider reaches the customer.
    /// </summary>
    public void MarkDelivered()
    {
        if (Status != OrderStatus.OutForDelivery)
            throw new DomainException($"Cannot mark order delivered when status is '{Status}'.", "NOT_OUT_FOR_DELIVERY");

        Status = OrderStatus.Delivered;
        DeliveredAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new OrderDeliveredDomainEvent(Id, DeliveredAtUtc.Value));
    }

    /// <summary>
    /// Cancels the order if it has not yet been delivered.
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Delivered)
            throw new DomainException("Cannot cancel an order that has already been delivered.", "ORDER_DELIVERED");

        if (Status == OrderStatus.Cancelled)
            throw new DomainException("Order is already cancelled.", "ORDER_ALREADY_CANCELLED");

        Status = OrderStatus.Cancelled;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? "No reason specified" : reason.Trim();
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new OrderCancelledDomainEvent(Id, CancellationReason, DateTime.UtcNow));
    }

    private void AssertCanModifyOrder()
    {
        if (Status != OrderStatus.PendingPayment)
            throw new DomainException($"Cannot modify items when order is in '{Status}' state.", "ORDER_LOCKED");
    }
}
