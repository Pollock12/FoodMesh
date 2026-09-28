using FoodMesh.Domain.Aggregates;

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
}
