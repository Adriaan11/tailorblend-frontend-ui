namespace BlazorConsultant.Services;

public class NotificationService : INotificationService
{
    public event EventHandler<NotificationEventArgs>? OnNotificationAdded;

    public void Add(string message, NotificationSeverity severity = NotificationSeverity.Info, int durationMs = 5000)
    {
        var notification = new NotificationEventArgs
        {
            Message = message,
            Severity = severity,
            DurationMs = durationMs
        };

        OnNotificationAdded?.Invoke(this, notification);
    }
}
