using FoodMesh.Domain.Entities;
using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using MediatR;
using MongoDB.Driver;

namespace FoodMesh.Read.QueryHandlers;

public sealed class GetAvailableRestaurantsQueryHandler : IRequestHandler<GetAvailableRestaurantsQuery, Result<IReadOnlyList<RestaurantSummaryViewModel>>>
{
    private readonly IMongoDatabase _database;

    public GetAvailableRestaurantsQueryHandler(IMongoDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<Result<IReadOnlyList<RestaurantSummaryViewModel>>> Handle(GetAvailableRestaurantsQuery request, CancellationToken cancellationToken)
    {
        var itemsCollection = _database.GetCollection<RestaurantItem>("RestaurantItems");

        var activeItems = await itemsCollection
            .Find(i => i.IsAvailable)
            .ToListAsync(cancellationToken);

        var summaries = activeItems
            .GroupBy(i => i.RestaurantId)
            .Select(g => new RestaurantSummaryViewModel
            {
                RestaurantId = g.Key,
                RestaurantName = $"Restaurant {g.Key.ToString()[..8]}",
                Cuisine = g.FirstOrDefault()?.Category ?? "Various",
                AvailableItemCount = g.Count()
            })
            .ToList();

        return Result<IReadOnlyList<RestaurantSummaryViewModel>>.Success(summaries);
    }
}
