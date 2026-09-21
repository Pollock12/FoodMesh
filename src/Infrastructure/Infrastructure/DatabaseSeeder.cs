using FoodMesh.Domain.Entities;
using FoodMesh.Domain.ValueObjects;
using MongoDB.Driver;

namespace FoodMesh.Infrastructure;

/// <summary>
/// Seeds initial restaurants, menu items, and delivery partners if database is fresh.
/// Ensures Swagger endpoints immediately have real data to query and test.
/// </summary>
public static class DatabaseSeeder
{
    public static readonly Guid BurgerBistroId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid PizzaPalaceId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static async Task SeedAsync(IMongoDatabase database)
    {
        var itemsCollection = database.GetCollection<RestaurantItem>("RestaurantItems");
        var partnersCollection = database.GetCollection<DeliveryPartner>("DeliveryPartners");

        // 1. Seed Restaurant Menu Items if empty
        var itemsCount = await itemsCollection.CountDocumentsAsync(FilterDefinition<RestaurantItem>.Empty);
        if (itemsCount == 0)
        {
            var seedItems = new List<RestaurantItem>
            {
                // Burger Bistro Menu
                new(
                    Guid.Parse("11111111-1111-1111-1111-000000000001"),
                    BurgerBistroId,
                    "Classic Cheeseburger",
                    "Juicy beef patty with aged cheddar, lettuce, tomato, and special sauce.",
                    Money.Create(12.99m, "USD"),
                    "Burgers",
                    true
                ),
                new(
                    Guid.Parse("11111111-1111-1111-1111-000000000002"),
                    BurgerBistroId,
                    "Smoky BBQ Bacon Burger",
                    "Grilled beef patty topped with smoked bacon, crispy onions, and tangy BBQ sauce.",
                    Money.Create(14.50m, "USD"),
                    "Burgers",
                    true
                ),
                new(
                    Guid.Parse("11111111-1111-1111-1111-000000000003"),
                    BurgerBistroId,
                    "Crispy French Fries",
                    "Golden, crunchy salted fries with garlic mayo dip.",
                    Money.Create(4.50m, "USD"),
                    "Sides",
                    true
                ),

                // Pizza Palace Menu
                new(
                    Guid.Parse("22222222-2222-2222-2222-000000000001"),
                    PizzaPalaceId,
                    "Margherita Pizza",
                    "Classic sourdough base with San Marzano tomatoes, fresh mozzarella, and basil.",
                    Money.Create(15.00m, "USD"),
                    "Pizza",
                    true
                ),
                new(
                    Guid.Parse("22222222-2222-2222-2222-000000000002"),
                    PizzaPalaceId,
                    "Pepperoni Feast",
                    "Loaded with artisanal beef pepperoni and melted mozzarella cheese.",
                    Money.Create(18.50m, "USD"),
                    "Pizza",
                    true
                ),
                new(
                    Guid.Parse("22222222-2222-2222-2222-000000000003"),
                    PizzaPalaceId,
                    "Cheesy Garlic Bread",
                    "Toasted baguette brushed with garlic butter and melted mozzarella.",
                    Money.Create(5.50m, "USD"),
                    "Sides",
                    true
                )
            };

            await itemsCollection.InsertManyAsync(seedItems);
        }

        // 2. Seed Delivery Partners (Riders) if empty
        var partnersCount = await partnersCollection.CountDocumentsAsync(FilterDefinition<DeliveryPartner>.Empty);
        if (partnersCount == 0)
        {
            var seedPartners = new List<DeliveryPartner>
            {
                new(
                    Guid.Parse("33333333-3333-3333-3333-000000000001"),
                    "John Doe (Rider)",
                    "+1-555-0101",
                    "Bike"
                ),
                new(
                    Guid.Parse("33333333-3333-3333-3333-000000000002"),
                    "Alex Smith (Rider)",
                    "+1-555-0102",
                    "Scooter"
                )
            };

            await partnersCollection.InsertManyAsync(seedPartners);
        }
    }
}
