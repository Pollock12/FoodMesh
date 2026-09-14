using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FoodMesh.Application.CommandWorker;

/// <summary>
/// Hosted background worker that drains and executes asynchronous background command tasks.
/// </summary>
public sealed class OrderProcessingBackgroundWorker : BackgroundService
{
    private readonly ICommandQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderProcessingBackgroundWorker> _logger;

    public OrderProcessingBackgroundWorker(
        ICommandQueue queue,
        IServiceProvider serviceProvider,
        ILogger<OrderProcessingBackgroundWorker> logger)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderProcessingBackgroundWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);
                await workItem(_serviceProvider, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when service is stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing background work item.");
            }
        }

        _logger.LogInformation("OrderProcessingBackgroundWorker is shutting down.");
    }
}
