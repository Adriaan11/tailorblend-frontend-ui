namespace BlazorConsultant.Services;

public interface INotificationService
{
    event EventHandler<NotificationEventArgs>? OnNotificationAdded;
    void Add(string message, NotificationSeverity severity = NotificationSeverity.Info, int durationMs = 5000);
}

public enum NotificationSeverity
{
    Info,
    Success,
    Warning,
    Error
}

public class NotificationEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;
    public NotificationSeverity Severity { get; set; }
    public int DurationMs { get; set; }
    public string Id { get; set; } = Guid.NewGuid().ToString();
}
