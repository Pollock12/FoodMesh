using System.Net;
using System.Text.Json;
using FoodMesh.Shared.Common;
using FoodMesh.Shared.SharedDto;

namespace FoodMesh.BusinessApiService.Middleware;

/// <summary>
/// Global exception handling middleware converting domain & application exceptions
/// into standardized ApiResponse error structures with appropriate HTTP status codes.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception caught by middleware: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                domainEx.Message,
                new List<string> { domainEx.ErrorCode ?? "DOMAIN_ERROR" }
            ),
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                argEx.Message,
                new List<string> { "INVALID_ARGUMENT" }
            ),
            KeyNotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                notFoundEx.Message,
                new List<string> { "NOT_FOUND" }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected internal error occurred. Please try again later.",
                new List<string> { "INTERNAL_SERVER_ERROR" }
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message, errors);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}
