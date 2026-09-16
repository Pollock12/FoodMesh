using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FoodMesh.Read.EventWorker;

/// <summary>
/// Background worker responsible for synchronizing read projections and keeping query caches warm.
/// Runs 24/7 as part of the CQRS read-side synchronization pipeline.
/// </summary>
public sealed class ReadProjectionSyncWorker : BackgroundService
{
    private readonly ILogger<ReadProjectionSyncWorker> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromSeconds(30);

    public ReadProjectionSyncWorker(ILogger<ReadProjectionSyncWorker> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReadProjectionSyncWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncProjectionsAsync(stoppingToken);
                await Task.Delay(_syncInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during read projection sync cycle.");
            }
        }

        _logger.LogInformation("ReadProjectionSyncWorker stopped.");
    }

    private Task SyncProjectionsAsync(CancellationToken cancellationToken)
    {
        // Periodic check for read projection consistency or cache invalidation
        return Task.CompletedTask;
    }
}
