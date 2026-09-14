namespace FoodMesh.Application.CommandWorker;

/// <summary>
/// Abstraction for consuming and executing a command message in the background engine room.
/// </summary>
public interface ICommandConsumer
{
    Task ConsumeAsync(object commandMessage, IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}
