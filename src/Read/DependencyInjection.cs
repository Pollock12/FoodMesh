using FoodMesh.Read.EventWorker;
using Microsoft.Extensions.DependencyInjection;

namespace FoodMesh.Read;

/// <summary>
/// Service collection extension methods to register Read layer components (Queries, EventHandlers, and Workers).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddReadLayer(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // 1. Register MediatR Query and Event Handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // 2. Register Read-Side Background Worker for projection syncing
        services.AddHostedService<ReadProjectionSyncWorker>();

        return services;
    }
}
