namespace BlazorConsultant.Services;

/// <summary>
/// Session management service implementation.
/// Scoped per SignalR connection - each user gets their own instance.
/// </summary>
public class SessionService : ISessionService
{
    private Guid _sessionId;
    private int _messageCount;
    private string _currentModel;

    public SessionService()
    {
        // Generate unique session ID on construction
        _sessionId = Guid.NewGuid();
        _messageCount = 0;
        _currentModel = "gpt-4.1-mini-2025-04-14"; // Default model

        Console.WriteLine($"✅ [SESSION] New session created: {_sessionId}");
    }

    public string SessionId => _sessionId.ToString();

    public int MessageCount => _messageCount;

    public string CurrentModel => _currentModel;

    public void IncrementMessageCount()
    {
        _messageCount++;
    }

    public void SetModel(string modelId)
    {
        _currentModel = modelId;
        Console.WriteLine($"🔄 [SESSION] Model changed to: {modelId}");
    }

    public void Reset()
    {
        // Generate new session ID on reset
        var oldSessionId = _sessionId;
        _sessionId = Guid.NewGuid();
        _messageCount = 0;
        Console.WriteLine($"🔄 [SESSION] Session reset: {oldSessionId} → {_sessionId}");
    }
}
