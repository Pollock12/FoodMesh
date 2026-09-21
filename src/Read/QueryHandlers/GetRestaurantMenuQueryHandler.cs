using FoodMesh.Domain.Entities;
using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using MediatR;
using MongoDB.Driver;

namespace FoodMesh.Read.QueryHandlers;

/// <summary>
/// Fast CQRS query handler fetching active menu items for a selected restaurant.
/// </summary>
public sealed class GetRestaurantMenuQueryHandler : IRequestHandler<GetRestaurantMenuQuery, Result<IReadOnlyList<MenuItemViewModel>>>
{
    private readonly IMongoDatabase _database;

    public GetRestaurantMenuQueryHandler(IMongoDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<Result<IReadOnlyList<MenuItemViewModel>>> Handle(GetRestaurantMenuQuery request, CancellationToken cancellationToken)
    {
        var itemsCollection = _database.GetCollection<RestaurantItem>("RestaurantItems");

        var filter = Builders<RestaurantItem>.Filter.And(
            Builders<RestaurantItem>.Filter.Eq(i => i.RestaurantId, request.RestaurantId),
            Builders<RestaurantItem>.Filter.Eq(i => i.IsAvailable, true));

        var items = await itemsCollection
            .Find(filter)
            .ToListAsync(cancellationToken);

        var viewModels = items.Select(i => new MenuItemViewModel
        {
            MenuItemId = i.Id,
            RestaurantId = i.RestaurantId,
            Name = i.Name,
            Description = i.Description,
            Price = i.Price.Amount,
            Currency = i.Price.Currency,
            Category = i.Category
        }).ToList();

        return Result<IReadOnlyList<MenuItemViewModel>>.Success(viewModels);
    }
}
