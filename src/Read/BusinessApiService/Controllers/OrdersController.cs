using FoodMesh.Application.Commands;
using FoodMesh.Read.QueryHandlers;
using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.SharedDto;
using Microsoft.AspNetCore.Mvc;

namespace FoodMesh.BusinessApiService.Controllers;

public sealed class CancelOrderRequest
{
    public string Reason { get; set; } = "Customer requested cancellation";
}

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : BaseApiController
{
    /// <summary>
    /// Places a new food order.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result, "Order placed successfully.");
    }

    /// <summary>
    /// Gets complete order details and current status by OrderId.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailsViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var result = await Mediator.Send(new GetOrderByIdQuery(id));
        return HandleResult(result);
    }

    /// <summary>
    /// Gets paginated order history for a specific customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderSummaryViewModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCustomerOrders(
        Guid customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetCustomerOrdersQuery(customerId, pageNumber, pageSize);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancels an order. Customer can ONLY cancel before payment (PendingPayment).
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
    {
        var command = new CancelOrderCommand(id, request.Reason);
        var result = await Mediator.Send(command);
        return HandleResult(result, "Order cancelled successfully.");
    }

    /// <summary>
    /// Updates order status to Cooking when the chef begins cooking in the kitchen.
    /// Notifies customer continuously of live kitchen progress.
    /// </summary>
    [HttpPost("{id:guid}/start-cooking")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartCooking(Guid id)
    {
        var result = await Mediator.Send(new StartCookingCommand(id));
        return HandleResult(result, "Cooking started successfully.");
    }

    /// <summary>
    /// Updates order status to OutForDelivery when the owner departs for delivery.
    /// In this self-operated restaurant, the owner delivers directly to the customer.
    /// </summary>
    [HttpPost("{id:guid}/dispatch")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DispatchDelivery(Guid id)
    {
        var result = await Mediator.Send(new DispatchDeliveryCommand(id));
        return HandleResult(result, "Order dispatched for delivery.");
    }

    /// <summary>
    /// Marks the order as Delivered upon reaching customer doorstep.
    /// Notifies customer that delivery has been completed.
    /// </summary>
    [HttpPost("{id:guid}/deliver")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteDelivery(Guid id)
    {
        var result = await Mediator.Send(new CompleteDeliveryCommand(id));
        return HandleResult(result, "Order marked as delivered successfully.");
    }
}
