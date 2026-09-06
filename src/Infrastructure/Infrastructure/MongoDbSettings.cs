namespace FoodMesh.Infrastructure;

/// <summary>
/// Configuration settings for connecting to MongoDB.
/// Typically bound from appsettings.json: "MongoDbSettings".
/// </summary>
public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDbSettings";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "FoodMeshDb";
}
