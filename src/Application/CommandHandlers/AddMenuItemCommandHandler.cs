using FoodMesh.Application.Commands;
using FoodMesh.Domain.Entities;
using FoodMesh.Domain.ValueObjects;
using FoodMesh.Infrastructure;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.CommandHandlers;

/// <summary>
/// Command handler for adding a new dish/item to the restaurant menu.
/// Executed by the restaurant owner.
/// </summary>
public sealed class AddMenuItemCommandHandler : IRequestHandler<AddMenuItemCommand, Result<Guid>>
{
    private readonly IMongoUnitOfWork _unitOfWork;

    public AddMenuItemCommandHandler(IMongoUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<Guid>> Handle(AddMenuItemCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<Guid>.Failure("Item name is required.");

        if (command.Price < 0)
            return Result<Guid>.Failure("Item price cannot be negative.");

        try
        {
            var item = new RestaurantItem(
                Guid.NewGuid(),
                command.Name,
                command.Description,
                new Money(command.Price, string.IsNullOrWhiteSpace(command.Currency) ? "USD" : command.Currency.Trim()),
                command.Category,
                command.IsAvailable);

            var itemRepo = _unitOfWork.GetRepository<RestaurantItem>();
            await itemRepo.InsertAsync(item, cancellationToken);

            return Result<Guid>.Success(item.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to add menu item: {ex.Message}");
        }
    }
}
