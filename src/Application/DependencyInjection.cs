using FluentValidation;
using FoodMesh.Application.CommandServices;
using FoodMesh.Application.CommandWorker;
using FoodMesh.Domain.DomainServices;
using FoodMesh.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FoodMesh.Application;

/// <summary>
/// Dependency injection registrations for the Application layer (Commands, Handlers, Services, and Workers).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // 1. Register MediatR handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // 2. Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // 3. Register Command Services (The Database Researchers & External Gateways)
        services.AddScoped<IOrderCommandService, OrderCommandService>();
        services.AddScoped<IPaymentGatewayClientService, PaymentGatewayClientService>();
        services.AddScoped<INotificationCommandService, NotificationCommandService>();

        // 4. Register Domain Services
        services.AddScoped<IDeliveryFeeCalculator, DeliveryFeeCalculator>();
        services.AddScoped<IOrderFulfillmentDomainService, OrderFulfillmentDomainService>();

        // 5. Register Background Command Queue & Worker
        services.AddSingleton<ICommandQueue, InMemoryCommandQueue>();
        services.AddHostedService<OrderProcessingBackgroundWorker>();

        return services;
    }
}
