using FoodMesh.Domain.Aggregates;
using FoodMesh.Domain.Entities;
using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using MediatR;
using MongoDB.Driver;

namespace FoodMesh.Read.QueryHandlers;

/// <summary>
/// Fast CQRS query handler reading directly from MongoDB collections into ViewModels.
/// Bypasses domain aggregate write behavior for high read performance.
/// </summary>
public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailsViewModel>>
{
    private readonly IMongoDatabase _database;

    public GetOrderByIdQueryHandler(IMongoDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<Result<OrderDetailsViewModel>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var ordersCollection = _database.GetCollection<Order>("Orders");
        var order = await ordersCollection
            .Find(o => o.Id == request.OrderId)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
            return Result<OrderDetailsViewModel>.Failure($"Order with ID '{request.OrderId}' not found.");

        string? partnerName = null;
        if (order.AssignedDeliveryPartnerId.HasValue)
        {
            var partnersCollection = _database.GetCollection<DeliveryPartner>("DeliveryPartners");
            var partner = await partnersCollection
                .Find(p => p.Id == order.AssignedDeliveryPartnerId.Value)
                .FirstOrDefaultAsync(cancellationToken);

            partnerName = partner?.FullName;
        }

        var viewModel = new OrderDetailsViewModel
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            RestaurantId = order.RestaurantId,
            Status = order.Status.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            PaymentTransactionId = order.PaymentTransactionId,
            AssignedDeliveryPartnerId = order.AssignedDeliveryPartnerId,
            DeliveryPartnerName = partnerName,
            DeliveryStreet = order.DeliveryAddress.Street,
            DeliveryCity = order.DeliveryAddress.City,
            DeliveryPostalCode = order.DeliveryAddress.PostalCode,
            ContactPhoneNumber = order.DeliveryAddress.ContactPhoneNumber,
            DeliveryLatitude = order.DeliveryAddress.Latitude,
            DeliveryLongitude = order.DeliveryAddress.Longitude,
            Subtotal = order.Subtotal.Amount,
            DeliveryFee = order.DeliveryFee.Amount,
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            PlacedAtUtc = order.PlacedAtUtc,
            PaidAtUtc = order.PaidAtUtc,
            DeliveredAtUtc = order.DeliveredAtUtc,
            CancellationReason = order.CancellationReason,
            Items = order.Items.Select(i => new OrderItemDetailsViewModel
            {
                MenuItemId = i.MenuItemId,
                ItemName = i.ItemName,
                UnitPrice = i.UnitPrice.Amount,
                Currency = i.UnitPrice.Currency,
                Quantity = i.Quantity
            }).ToList()
        };

        return Result<OrderDetailsViewModel>.Success(viewModel);
    }
}
