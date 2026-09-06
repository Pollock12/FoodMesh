using FoodMesh.Domain.ValueObjects;
using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Entities;

/// <summary>
/// Entity representing an item within an Order.
/// Belongs to the Order Aggregate Root boundary.
/// </summary>
public sealed class OrderItem : Entity<Guid>
{
    public Guid MenuItemId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public Money UnitPrice { get; private set; } = default!;
    public int Quantity { get; private set; }

    public Money Subtotal => UnitPrice * Quantity;

    private OrderItem() : base() { }

    public OrderItem(Guid id, Guid menuItemId, string itemName, Money unitPrice, int quantity) : base(id)
    {
        if (menuItemId == Guid.Empty)
            throw new DomainException("Menu item identifier cannot be empty.", "INVALID_ITEM_ID");

        if (string.IsNullOrWhiteSpace(itemName))
            throw new DomainException("Item name cannot be empty.", "INVALID_ITEM_NAME");

        if (unitPrice is null || unitPrice.Amount <= 0)
            throw new DomainException("Item unit price must be greater than zero.", "INVALID_UNIT_PRICE");

        if (quantity <= 0)
            throw new DomainException("Quantity must be at least 1.", "INVALID_QUANTITY");

        MenuItemId = menuItemId;
        ItemName = itemName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be at least 1.", "INVALID_QUANTITY");

        Quantity = newQuantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
