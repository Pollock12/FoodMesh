namespace FoodMesh.Application.CommandServices;

/// <summary>
/// Service abstraction for dispatching customer and partner notifications.
/// </summary>
public interface INotificationCommandService
{
    Task NotifyCustomerAsync(Guid customerId, string title, string message, CancellationToken cancellationToken = default);
    Task NotifyRiderAsync(Guid riderId, string title, string message, CancellationToken cancellationToken = default);
}
