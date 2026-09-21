using Microsoft.Extensions.DependencyInjection;

namespace FoodMesh.Read;

/// <summary>
/// Service collection extension methods to register Read layer components (Queries and EventHandlers).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddReadLayer(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR Query and Event Handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        return services;
    }
}
