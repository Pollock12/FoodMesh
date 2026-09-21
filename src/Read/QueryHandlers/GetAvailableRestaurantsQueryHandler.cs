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
            .Select(g =>
            {
                var firstItem = g.FirstOrDefault();
                string name = g.Key.ToString() switch
                {
                    "11111111-1111-1111-1111-111111111111" => "Burger Bistro",
                    "22222222-2222-2222-2222-222222222222" => "Pizza Palace",
                    _ => $"Restaurant {g.Key.ToString()[..8]}"
                };

                return new RestaurantSummaryViewModel
                {
                    RestaurantId = g.Key,
                    RestaurantName = name,
                    Cuisine = firstItem?.Category ?? "Various",
                    AvailableItemCount = g.Count()
                };
            })
            .ToList();

        return Result<IReadOnlyList<RestaurantSummaryViewModel>>.Success(summaries);
    }
}
