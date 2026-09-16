using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Read.QueryHandlers;

/// <summary>
/// Query to fetch real-time delivery tracking and rider GPS coordinates for an order.
/// </summary>
public sealed record TrackDeliveryQuery(Guid OrderId) : IRequest<Result<ActiveDeliveryTrackingDto>>;
