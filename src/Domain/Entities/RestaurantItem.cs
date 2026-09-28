using FoodMesh.Domain.ValueObjects;
using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Entities;

/// <summary>
/// Entity representing a food item offered on the restaurant menu.
/// Supports soft delete so historical orders maintain item integrity.
/// </summary>
public sealed class RestaurantItem : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Money Price { get; private set; } = default!;
    public string Category { get; private set; } = string.Empty;
    public bool IsAvailable { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    private RestaurantItem() : base() { }

    public RestaurantItem(
        Guid id,
        string name,
        string description,
        Money price,
        string category,
        bool isAvailable = true,
        bool isDeleted = false) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Item name cannot be empty.", "INVALID_NAME");

        if (price is null || price.Amount < 0)
            throw new DomainException("Price cannot be negative.", "INVALID_PRICE");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        Category = category?.Trim() ?? "General";
        IsAvailable = isAvailable;
        IsDeleted = isDeleted;
    }

    public void SetAvailability(bool isAvailable)
    {
        if (IsDeleted && isAvailable)
            throw new DomainException("Cannot mark a deleted menu item as available.", "CANNOT_ENABLE_DELETED_ITEM");

        IsAvailable = isAvailable;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdatePrice(Money newPrice)
    {
        if (newPrice is null || newPrice.Amount < 0)
            throw new DomainException("Price cannot be negative.", "INVALID_PRICE");

        Price = newPrice;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft deletes the menu item, preventing it from appearing in customer queries or new orders.
    /// </summary>
    public void SoftDelete()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsAvailable = false;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Restores a previously soft-deleted menu item.
    /// </summary>
    public void Restore()
    {
        if (!IsDeleted) return;

        IsDeleted = false;
        IsAvailable = true;
        DeletedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
