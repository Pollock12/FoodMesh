using FoodMesh.Application.Commands;
using FoodMesh.Shared.SharedDto;
using Microsoft.AspNetCore.Mvc;

namespace FoodMesh.BusinessApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DeliveriesController : BaseApiController
{
    /// <summary>
    /// Finds and assigns the nearest available delivery partner to an order.
    /// </summary>
    [HttpPost("assign")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignDeliveryPartner([FromBody] AssignDeliveryPartnerCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result, "Delivery partner assigned successfully.");
    }
}
