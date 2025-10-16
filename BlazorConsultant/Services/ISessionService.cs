namespace BlazorConsultant.Services;

/// <summary>
/// Session management service interface.
/// Maintains session state per SignalR connection (scoped).
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Unique session identifier for this Blazor Server connection.
    /// </summary>
    string SessionId { get; }

    /// <summary>
    /// Number of messages sent in this session.
    /// </summary>
    int MessageCount { get; }

    /// <summary>
    /// Current AI model being used.
    /// </summary>
    string CurrentModel { get; }

    /// <summary>
    /// Increment message counter.
    /// </summary>
    void IncrementMessageCount();

    /// <summary>
    /// Set the current AI model.
    /// </summary>
    void SetModel(string modelId);

    /// <summary>
    /// Reset session state.
    /// </summary>
    void Reset();
}
