using FoodMesh.Application;
using FoodMesh.BusinessApiService.Middleware;
using FoodMesh.Infrastructure;
using FoodMesh.Read;

var builder = WebApplication.CreateBuilder(args);

// Cloud deployment: Bind to PORT environment variable if provided by host (e.g. Render, Railway)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

// 1. Register Controllers & JSON formatting
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// 2. Register Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FoodMesh Business API",
        Version = "v1",
        Description = "Enterprise food ordering, payment, and delivery platform API built with .NET 8, CQRS, DDD, and MongoDB."
    });
});

// 3. Register Clean Architecture Layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddReadLayer();

// 4. Enable CORS for frontend clients (Angular / React)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 5. Middleware Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FoodMesh API v1");
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Redirect root URL "/" to "/swagger" so both URLs work seamlessly (excluded from Swagger UI)
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// 6. Seed initial restaurants, menus, and riders if database is fresh
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<MongoDB.Driver.IMongoDatabase>();
    await DatabaseSeeder.SeedAsync(database);
}

app.Run();

// Partial program class for WebApplicationFactory testing
public partial class Program { }

// Request -> Middleware -> Controller -> Handler -> Response
