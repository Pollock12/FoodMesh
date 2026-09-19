using FoodMesh.Application.Commands;
using FoodMesh.Read.QueryHandlers;
using FoodMesh.Read.ViewModels;
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

    /// <summary>
    /// Tracks live delivery coordinates and status for an order.
    /// </summary>
    [HttpGet("{orderId:guid}/track")]
    [ProducesResponseType(typeof(ApiResponse<ActiveDeliveryTrackingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrackDelivery(Guid orderId)
    {
        var result = await Mediator.Send(new TrackDeliveryQuery(orderId));
        return HandleResult(result);
    }
}
