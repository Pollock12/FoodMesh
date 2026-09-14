namespace FoodMesh.Application.CommandServices;

/// <summary>
/// Default implementation of notification dispatcher.
/// </summary>
public sealed class NotificationCommandService : INotificationCommandService
{
    public Task NotifyCustomerAsync(Guid customerId, string title, string message, CancellationToken cancellationToken = default)
    {
        // In a real production system, this sends an FCM push notification, Twilio SMS, or SendGrid email.
        return Task.CompletedTask;
    }

    public Task NotifyRiderAsync(Guid riderId, string title, string message, CancellationToken cancellationToken = default)
    {
        // In a real production system, this notifies the rider via WebSocket / Rider App push notification.
        return Task.CompletedTask;
    }
}
