using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Read.QueryHandlers;

/// <summary>
/// Query to fetch available menu items for a specific restaurant.
/// </summary>
public sealed record GetRestaurantMenuQuery(Guid RestaurantId) : IRequest<Result<IReadOnlyList<MenuItemViewModel>>>;
