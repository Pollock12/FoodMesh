namespace FoodMesh.Application.CommandServices;

/// <summary>
/// Default implementation of notification dispatcher.
/// </summary>
public sealed class NotificationCommandService : INotificationCommandService
{
    public Task NotifyCustomerAsync(Guid customerId, string title, string message, CancellationToken cancellationToken = default)
    {
        // In production, sends real-time updates via SignalR, FCM push notification, or SMS.
        return Task.CompletedTask;
    }

    public Task NotifyKitchenAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        // In production, notifies the restaurant kitchen dashboard or sound alert.
        return Task.CompletedTask;
    }
}
