using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Read.QueryHandlers;

/// <summary>
/// Query to fetch complete details of an order by its ID.
/// </summary>
public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderDetailsViewModel>>;
