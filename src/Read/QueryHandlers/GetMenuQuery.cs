using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Read.QueryHandlers;

/// <summary>
/// Public query to fetch the available menu items of the restaurant.
/// Can be called by anyone (customers, guests, owner).
/// </summary>
public sealed record GetMenuQuery(string? Category = null) : IRequest<Result<IReadOnlyList<MenuItemViewModel>>>;
