using FoodMesh.Read.QueryHandlers;
using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.SharedDto;
using Microsoft.AspNetCore.Mvc;

namespace FoodMesh.BusinessApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RestaurantsController : BaseApiController
{
    /// <summary>
    /// Gets a list of active restaurants with available menu counts.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RestaurantSummaryViewModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRestaurants()
    {
        var result = await Mediator.Send(new GetAvailableRestaurantsQuery());
        return HandleResult(result);
    }
}
