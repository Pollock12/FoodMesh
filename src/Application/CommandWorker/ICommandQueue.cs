namespace FoodMesh.Application.CommandWorker;

/// <summary>
/// In-memory queue abstraction for asynchronous background command dispatching.
/// </summary>
public interface ICommandQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Func<IServiceProvider, CancellationToken, ValueTask> workItem);
    ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}
