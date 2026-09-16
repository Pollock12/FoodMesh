using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Read.QueryHandlers;

/// <summary>
/// Query to fetch available restaurants and catalog counts.
/// </summary>
public sealed record GetAvailableRestaurantsQuery : IRequest<Result<IReadOnlyList<RestaurantSummaryDto>>>;
