using FoodMesh.Domain.Entities;
using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using MediatR;
using MongoDB.Driver;

namespace FoodMesh.Read.QueryHandlers;

public sealed class GetAvailableRestaurantsQueryHandler : IRequestHandler<GetAvailableRestaurantsQuery, Result<IReadOnlyList<RestaurantSummaryDto>>>
{
    private readonly IMongoDatabase _database;

    public GetAvailableRestaurantsQueryHandler(IMongoDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<Result<IReadOnlyList<RestaurantSummaryDto>>> Handle(GetAvailableRestaurantsQuery request, CancellationToken cancellationToken)
    {
        var itemsCollection = _database.GetCollection<RestaurantItem>("RestaurantItems");

        var activeItems = await itemsCollection
            .Find(i => i.IsAvailable)
            .ToListAsync(cancellationToken);

        var summaries = activeItems
            .GroupBy(i => i.RestaurantId)
            .Select(g => new RestaurantSummaryDto
            {
                RestaurantId = g.Key,
                RestaurantName = $"Restaurant {g.Key.ToString()[..8]}",
                Cuisine = g.FirstOrDefault()?.Category ?? "Various",
                AvailableItemCount = g.Count(),
                Latitude = 23.8103, // Default city center reference
                Longitude = 90.4125
            })
            .ToList();

        return Result<IReadOnlyList<RestaurantSummaryDto>>.Success(summaries);
    }
}
