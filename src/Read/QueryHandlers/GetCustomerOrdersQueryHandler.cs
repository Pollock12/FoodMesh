using FoodMesh.Domain.Aggregates;
using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using FoodMesh.Shared.SharedDto;
using MediatR;
using MongoDB.Driver;

namespace FoodMesh.Read.QueryHandlers;

public sealed class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, Result<PagedResult<OrderSummaryDto>>>
{
    private readonly IMongoDatabase _database;

    public GetCustomerOrdersQueryHandler(IMongoDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 50);

        var ordersCollection = _database.GetCollection<Order>("Orders");
        var filter = Builders<Order>.Filter.Eq(o => o.CustomerId, request.CustomerId);

        var totalCount = await ordersCollection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var orders = await ordersCollection
            .Find(filter)
            .SortByDescending(o => o.PlacedAtUtc)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        var items = orders.Select(o => new OrderSummaryDto
        {
            OrderId = o.Id,
            CustomerId = o.CustomerId,
            RestaurantId = o.RestaurantId,
            Status = o.Status.ToString(),
            ItemCount = o.Items.Count,
            TotalAmount = o.TotalAmount.Amount,
            Currency = o.TotalAmount.Currency,
            PlacedAtUtc = o.PlacedAtUtc
        }).ToList();

        var pagedResult = new PagedResult<OrderSummaryDto>(items, page, pageSize, totalCount);
        return Result<PagedResult<OrderSummaryDto>>.Success(pagedResult);
    }
}
