using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.Entities;
using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using MediatR;
using MongoDB.Driver;

namespace FoodMesh.Read.QueryHandlers;

public sealed class TrackDeliveryQueryHandler : IRequestHandler<TrackDeliveryQuery, Result<ActiveDeliveryTrackingViewModel>>
{
    private readonly IMongoDatabase _database;

    public TrackDeliveryQueryHandler(IMongoDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<Result<ActiveDeliveryTrackingViewModel>> Handle(TrackDeliveryQuery request, CancellationToken cancellationToken)
    {
        var ordersCollection = _database.GetCollection<Order>("Orders");
        var order = await ordersCollection
            .Find(o => o.Id == request.OrderId)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
            return Result<ActiveDeliveryTrackingViewModel>.Failure($"Order with ID '{request.OrderId}' not found.");

        DeliveryPartner? rider = null;
        if (order.AssignedDeliveryPartnerId.HasValue)
        {
            var partnersCollection = _database.GetCollection<DeliveryPartner>("DeliveryPartners");
            rider = await partnersCollection
                .Find(p => p.Id == order.AssignedDeliveryPartnerId.Value)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var trackingViewModel = new ActiveDeliveryTrackingViewModel
        {
            OrderId = order.Id,
            OrderStatus = order.Status.ToString(),
            DeliveryPartnerId = order.AssignedDeliveryPartnerId,
            RiderName = rider?.FullName,
            RiderPhone = rider?.PhoneNumber,
            VehicleType = rider?.VehicleType,
            RiderLatitude = rider?.CurrentLatitude ?? 0.0,
            RiderLongitude = rider?.CurrentLongitude ?? 0.0,
            DestinationLatitude = order.DeliveryAddress.Latitude,
            DestinationLongitude = order.DeliveryAddress.Longitude,
            DeliveryAddress = order.DeliveryAddress.ToString(),
            LastUpdatedUtc = rider?.UpdatedAtUtc ?? order.UpdatedAtUtc ?? order.PlacedAtUtc
        };

        return Result<ActiveDeliveryTrackingViewModel>.Success(trackingViewModel);
    }
}
