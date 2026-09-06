using FoodMesh.Domain.ValueObjects;
using FoodMesh.Shared.Common;

namespace FoodMesh.Domain.Entities;

/// <summary>
/// Entity representing a food item offered by a Restaurant menu.
/// </summary>
public sealed class RestaurantItem : Entity<Guid>
{
    public Guid RestaurantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Money Price { get; private set; } = default!;
    public string Category { get; private set; } = string.Empty;
    public bool IsAvailable { get; private set; }

    private RestaurantItem() : base() { }

    public RestaurantItem(
        Guid id,
        Guid restaurantId,
        string name,
        string description,
        Money price,
        string category,
        bool isAvailable = true) : base(id)
    {
        if (restaurantId == Guid.Empty)
            throw new DomainException("Restaurant identifier cannot be empty.", "INVALID_RESTAURANT_ID");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Item name cannot be empty.", "INVALID_NAME");

        if (price is null || price.Amount < 0)
            throw new DomainException("Price cannot be negative.", "INVALID_PRICE");

        RestaurantId = restaurantId;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        Category = category?.Trim() ?? "General";
        IsAvailable = isAvailable;
    }

    public void SetAvailability(bool isAvailable)
    {
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
}
