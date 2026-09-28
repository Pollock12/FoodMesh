using FoodMesh.Shared.Common;
using MediatR;

namespace FoodMesh.Application.Commands;

/// <summary>
/// Command for the restaurant owner to add a new food item to the menu.
/// </summary>
public sealed record AddMenuItemCommand(
    string Name,
    string Description,
    decimal Price,
    string Currency = "USD",
    string Category = "General",
    bool IsAvailable = true) : IRequest<Result<Guid>>;
