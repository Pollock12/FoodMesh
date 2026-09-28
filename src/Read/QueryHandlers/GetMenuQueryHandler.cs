using FoodMesh.Domain.Entities;
using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using MediatR;
using MongoDB.Driver;

namespace FoodMesh.Read.QueryHandlers;

/// <summary>
/// Fast read query handler fetching the restaurant menu items.
/// Open for public access.
/// </summary>
public sealed class GetMenuQueryHandler : IRequestHandler<GetMenuQuery, Result<IReadOnlyList<MenuItemViewModel>>>
{
    private readonly IMongoDatabase _database;

    public GetMenuQueryHandler(IMongoDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<Result<IReadOnlyList<MenuItemViewModel>>> Handle(GetMenuQuery request, CancellationToken cancellationToken)
    {
        var itemsCollection = _database.GetCollection<RestaurantItem>("RestaurantItems");

        var filterBuilder = Builders<RestaurantItem>.Filter;
        var filter = filterBuilder.Eq(i => i.IsAvailable, true);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            filter = filterBuilder.And(filter, filterBuilder.Eq(i => i.Category, request.Category.Trim()));
        }

        var items = await itemsCollection
            .Find(filter)
            .ToListAsync(cancellationToken);

        var viewModels = items.Select(i => new MenuItemViewModel
        {
            MenuItemId = i.Id,
            Name = i.Name,
            Description = i.Description,
            Price = i.Price.Amount,
            Currency = i.Price.Currency,
            Category = i.Category
        }).ToList();

        return Result<IReadOnlyList<MenuItemViewModel>>.Success(viewModels);
    }
}
