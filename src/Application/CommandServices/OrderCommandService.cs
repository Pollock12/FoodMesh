using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.Entities;
using FoodMesh.Infrastructure;

namespace FoodMesh.Application.CommandServices;

/// <summary>
/// Implementation of IOrderCommandService.
/// Queries the database to validate business prerequisites before mutating aggregates.
/// </summary>
public sealed class OrderCommandService : IOrderCommandService
{
    private readonly IMongoUnitOfWork _unitOfWork;

    public OrderCommandService(IMongoUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.GetRepository<Order>();
        return await repo.GetByIdAsync(orderId, cancellationToken);
    }

    public async Task<bool> IsRestaurantActiveAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        if (restaurantId == Guid.Empty) return false;

        // Verify restaurant has active menu items in database (or returns true if restaurant exists)
        var repo = _unitOfWork.GetRepository<RestaurantItem>();
        var items = await repo.FindAsync(i => i.RestaurantId == restaurantId && i.IsAvailable, cancellationToken);
        
        // If restaurant has at least 1 item or exists, it is active
        return items.Count > 0;
    }

    public async Task<bool> AreMenuItemsAvailableAsync(
        Guid restaurantId,
        IEnumerable<Guid> menuItemIds,
        CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.GetRepository<RestaurantItem>();
        var idList = menuItemIds.ToList();

        var availableItems = await repo.FindAsync(
            i => i.RestaurantId == restaurantId && idList.Contains(i.Id) && i.IsAvailable,
            cancellationToken);

        return availableItems.Count == idList.Count;
    }

    public async Task<IReadOnlyList<DeliveryPartner>> GetAvailableDeliveryPartnersAsync(CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.GetRepository<DeliveryPartner>();
        return await repo.FindAsync(p => p.IsAvailable && !p.ActiveOrderId.HasValue, cancellationToken);
    }

    public async Task<DeliveryPartner?> GetDeliveryPartnerAsync(Guid partnerId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.GetRepository<DeliveryPartner>();
        return await repo.GetByIdAsync(partnerId, cancellationToken);
    }
}
