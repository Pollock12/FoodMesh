using System.Net;
using FluentAssertions;
using FoodMesh.Application.Commands;
using FoodMesh.Application.DataMappers;
using FoodMesh.BusinessApiService.Controllers;
using FoodMesh.BusinessApiService.Middleware;
using FoodMesh.Read.QueryHandlers;
using FoodMesh.Read.ViewModels;
using FoodMesh.Shared.Common;
using FoodMesh.Shared.SharedDto;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FoodMesh.Domain.Tests;

public class ApiControllerTests
{
    private readonly Mock<ISender> _mockMediator = new();

    private TController SetupController<TController>(TController controller) where TController : BaseApiController
    {
        var httpContext = new DefaultHttpContext();
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(sp => sp.GetService(typeof(ISender))).Returns(_mockMediator.Object);
        httpContext.RequestServices = serviceProvider.Object;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public async Task OrdersController_CreateOrder_Should_Return_Ok_On_Success()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(orderId));

        var controller = SetupController(new OrdersController());
        var command = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            RestaurantId: Guid.NewGuid(),
            DeliveryAddress: new DeliveryAddressDto { Street = "A", City = "B", ContactPhoneNumber = "123" },
            Items: [new OrderItemDto { MenuItemId = Guid.NewGuid(), ItemName = "Pizza", UnitPrice = 15m, Quantity = 1 }]);

        // Act
        var result = await controller.CreateOrder(command);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<Guid>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().Be(orderId);
    }

    [Fact]
    public async Task OrdersController_CreateOrder_Should_Return_BadRequest_On_Failure()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Failure("Restaurant is closed"));

        var controller = SetupController(new OrdersController());
        var command = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            RestaurantId: Guid.NewGuid(),
            DeliveryAddress: new DeliveryAddressDto { Street = "A", City = "B", ContactPhoneNumber = "123" },
            Items: []);

        // Act
        var result = await controller.CreateOrder(command);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<ApiResponse<Guid>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Restaurant is closed");
    }

    [Fact]
    public async Task PaymentsController_ProcessPayment_Should_Return_Ok_With_TransactionId()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<ProcessPaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("TXN_123456"));

        var controller = SetupController(new PaymentsController());
        var command = new ProcessPaymentCommand(Guid.NewGuid(), "CreditCard");

        // Act
        var result = await controller.ProcessPayment(command);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().Be("TXN_123456");
    }

    [Fact]
    public async Task DeliveriesController_AssignDeliveryPartner_Should_Return_Ok_With_PartnerId()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        _mockMediator
            .Setup(m => m.Send(It.IsAny<AssignDeliveryPartnerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(partnerId));

        var controller = SetupController(new DeliveriesController());
        var command = new AssignDeliveryPartnerCommand(Guid.NewGuid());

        // Act
        var result = await controller.AssignDeliveryPartner(command);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<Guid>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().Be(partnerId);
    }


    [Fact]
    public async Task ExceptionHandlingMiddleware_Should_Catch_DomainException_And_Return_BadRequest()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        RequestDelegate next = _ => throw new DomainException("Cannot cancel a delivered order.", "ORDER_DELIVERED");

        var middleware = new ExceptionHandlingMiddleware(next, mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Contain("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("Cannot cancel a delivered order.");
        responseBody.Should().Contain("ORDER_DELIVERED");
    }
}
