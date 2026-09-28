using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.Commands;

/// <summary>
/// Command to soft-delete a menu item from the restaurant catalog.
/// Executed by the restaurant owner.
/// </summary>
public sealed record DeleteMenuItemCommand(Guid MenuItemId) : IRequest<Result<bool>>;
