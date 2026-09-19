using FoodMesh.Application.Commands;
using FoodMesh.Shared.SharedDto;
using Microsoft.AspNetCore.Mvc;

namespace FoodMesh.BusinessApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PaymentsController : BaseApiController
{
    /// <summary>
    /// Processes payment for an order and triggers food preparation.
    /// </summary>
    [HttpPost("process")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result, "Payment processed successfully.");
    }
}
