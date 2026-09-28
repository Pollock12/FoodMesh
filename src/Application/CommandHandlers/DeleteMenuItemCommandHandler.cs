using FoodMesh.Application.Commands;
using FoodMesh.Domain.Entities;
using FoodMesh.Infrastructure;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.CommandHandlers;

/// <summary>
/// Command handler that applies a soft delete to an existing RestaurantItem.
/// </summary>
public sealed class DeleteMenuItemCommandHandler : IRequestHandler<DeleteMenuItemCommand, Result<bool>>
{
    private readonly IMongoUnitOfWork _unitOfWork;

    public DeleteMenuItemCommandHandler(IMongoUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<bool>> Handle(DeleteMenuItemCommand command, CancellationToken cancellationToken)
    {
        if (command.MenuItemId == Guid.Empty)
            return Result<bool>.Failure("Menu item ID cannot be empty.");

        var repo = _unitOfWork.GetRepository<RestaurantItem>();
        var item = await repo.GetByIdAsync(command.MenuItemId, cancellationToken);

        if (item is null || item.IsDeleted)
            return Result<bool>.Failure($"Menu item with ID '{command.MenuItemId}' not found.");

        try
        {
            item.SoftDelete();
            await repo.UpdateAsync(item, cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete menu item: {ex.Message}");
        }
    }
}
