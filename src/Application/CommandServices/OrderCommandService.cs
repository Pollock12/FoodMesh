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

    public async Task<bool> AreMenuItemsAvailableAsync(
        IEnumerable<Guid> menuItemIds,
        CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.GetRepository<RestaurantItem>();
        var idList = menuItemIds.ToList();

        var availableItems = await repo.FindAsync(
            i => idList.Contains(i.Id) && i.IsAvailable && !i.IsDeleted,
            cancellationToken);

        return availableItems.Count == idList.Count;
    }
}
