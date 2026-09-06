using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FoodMesh.Infrastructure;

/// <summary>
/// Service collection extension methods to register Infrastructure services and MongoDB persistence.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Register MongoDB BSON Mappings for Domain types
        BsonClassMaps.Register();

        // 2. Configure MongoDB Settings
        services.Configure<MongoDbSettings>(
            configuration.GetSection(MongoDbSettings.SectionName));

        // 3. Register MongoClient (Singleton per application)
        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        // 4. Register IMongoDatabase (Scoped)
        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return client.GetDatabase(settings.DatabaseName);
        });

        // 5. Register Unit of Work (Scoped)
        services.AddScoped<IMongoUnitOfWork, MongoUnitOfWork>();

        // 6. Register Generic Transactional Repository (Scoped)
        services.AddScoped(typeof(ITransactionalRepository<>), typeof(TransactionalRepository<>));

        return services;
    }
}
