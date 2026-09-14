using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.Entities;

namespace FoodMesh.Application.CommandServices;

/// <summary>
/// Command service acting as the "Database Researcher/Checker" for the write handlers.
/// Performs pre-execution cross-entity lookups and business validation queries before command execution.
/// </summary>
public interface IOrderCommandService
{
    Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<bool> IsRestaurantActiveAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<bool> AreMenuItemsAvailableAsync(Guid restaurantId, IEnumerable<Guid> menuItemIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeliveryPartner>> GetAvailableDeliveryPartnersAsync(CancellationToken cancellationToken = default);
    Task<DeliveryPartner?> GetDeliveryPartnerAsync(Guid partnerId, CancellationToken cancellationToken = default);
}
