using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.ValueObjects;
using FoodMesh.Shared.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace FoodMesh.Infrastructure;

/// <summary>
/// Registers MongoDB BSON mappings only for types requiring custom mapping:
/// 1. AggregateRoot<Guid> (unmapping DomainEvents so they are not persisted)
/// 2. Order (mapping private backing field _items)
/// 3. Money (mapping constructor-only parameters)
/// 4. DeliveryAddress (mapping constructor-only parameters)
/// All other entities (Entity, OrderItem, RestaurantItem, DeliveryPartner) are handled automatically by MongoDB conventions.
/// </summary>
public static class BsonClassMaps
{
    private static bool _isRegistered;
    private static readonly object _syncRoot = new();

    public static void Register()
    {
        if (_isRegistered) return;

        lock (_syncRoot)
        {
            if (_isRegistered) return;

            // Register standard Guid representation so MongoDB can serialize Guid identifiers and properties
            try
            {
                BsonSerializer.RegisterSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));
            }
            catch (BsonSerializationException)
            {
                // Already registered
            }

            // 1. AggregateRoot: DomainEvents must never be persisted to the database
            if (!BsonClassMap.IsClassMapRegistered(typeof(AggregateRoot<Guid>)))
            {
                BsonClassMap.RegisterClassMap<AggregateRoot<Guid>>(cm =>
                {
                    cm.AutoMap();
                    cm.UnmapMember(c => c.DomainEvents);
                });
            }

            // 2. Order Aggregate Root: Map private backing field '_items' to BSON element 'Items'
            if (!BsonClassMap.IsClassMapRegistered(typeof(Order)))
            {
                BsonClassMap.RegisterClassMap<Order>(cm =>
                {
                    cm.AutoMap();
                    cm.UnmapMember(c => c.Items);
                    cm.MapField("_items").SetElementName("Items");
                });
            }

            // 3. Money Value Object: Immutable with constructor-only arguments
            if (!BsonClassMap.IsClassMapRegistered(typeof(Money)))
            {
                BsonClassMap.RegisterClassMap<Money>(cm =>
                {
                    cm.AutoMap();
                    cm.MapCreator(m => new Money(m.Amount, m.Currency));
                });
            }

            // 4. DeliveryAddress Value Object: Immutable with constructor-only arguments
            if (!BsonClassMap.IsClassMapRegistered(typeof(DeliveryAddress)))
            {
                BsonClassMap.RegisterClassMap<DeliveryAddress>(cm =>
                {
                    cm.AutoMap();
                    cm.MapCreator(a => new DeliveryAddress(
                        a.Street,
                        a.City,
                        a.PostalCode,
                        a.ContactPhoneNumber,
                        a.Latitude,
                        a.Longitude));
                });
            }

            _isRegistered = true;
        }
    }
}
