using FoodMesh.Application.Commands;
using FoodMesh.Read.QueryHandlers;
using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.SharedDto;
using Microsoft.AspNetCore.Mvc;

namespace FoodMesh.BusinessApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MenuController : BaseApiController
{
    /// <summary>
    /// Gets all available food items on the restaurant menu.
    /// Public endpoint accessible by anyone.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MenuItemViewModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMenu([FromQuery] string? category = null)
    {
        var result = await Mediator.Send(new GetMenuQuery(category));
        return HandleResult(result);
    }

    /// <summary>
    /// Adds a new food dish to the menu.
    /// Executed by the restaurant owner.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddMenuItem([FromBody] AddMenuItemCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result, "Menu item added successfully.");
    }

    /// <summary>
    /// Soft deletes a food item from the menu.
    /// Executed by the restaurant owner.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMenuItem([FromRoute] Guid id)
    {
        var result = await Mediator.Send(new DeleteMenuItemCommand(id));
        return HandleResult(result, "Menu item removed successfully.");
    }
}
