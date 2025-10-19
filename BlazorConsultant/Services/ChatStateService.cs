using BlazorConsultant.Models;

namespace BlazorConsultant.Services;

/// <summary>
/// Chat state management service implementation.
/// Maintains conversation state across navigation.
/// Scoped per SignalR connection.
/// </summary>
public class ChatStateService : IChatStateService
{
    private readonly IChatService _chatService;
    private readonly ISessionService _sessionService;
    private readonly List<ChatMessage> _messages = new();
    private bool _isLoading;
    private bool _disposed;

    public ChatStateService(
        IChatService chatService,
        ISessionService sessionService)
    {
        _chatService = chatService;
        _sessionService = sessionService;
    }

    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

    public bool IsLoading => _isLoading;

    public event EventHandler? OnStateChanged;

    public async Task SendMessageAsync(string message, List<FileAttachment>? attachments = null)
    {
        if (string.IsNullOrWhiteSpace(message) || _isLoading)
            return;

        // Add user message immediately
        var userMessage = new ChatMessage
        {
            Role = "user",
            Content = message.Trim(),
            Timestamp = DateTime.Now,
            Attachments = attachments ?? new List<FileAttachment>()
        };
        _messages.Add(userMessage);

        _isLoading = true;
        NotifyStateChanged();

        try
        {
            Console.WriteLine($"⏳ [ChatStateService] Sending message...");

            var response = await _chatService.SendChatAsync(
                message,
                customInstructions: null,
                model: _sessionService.CurrentModel,
                attachments: attachments,
                practitionerMode: false
            );

            Console.WriteLine($"✅ [ChatStateService] Received {response.Response.Length} chars");

            // Add assistant message immediately
            var assistantMessage = new ChatMessage
            {
                Role = "assistant",
                Content = response.Response,
                Timestamp = DateTime.Now
            };
            _messages.Add(assistantMessage);

            // Increment message counter
            _sessionService.IncrementMessageCount();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [ChatStateService] Request failed: {ex.Message}");

            var errorMessage = new ChatMessage
            {
                Role = "assistant",
                Content = $"⚠️ Sorry, we're having trouble connecting. Please try again.",
                Timestamp = DateTime.Now
            };
            _messages.Add(errorMessage);
        }
        finally
        {
            _isLoading = false;
            NotifyStateChanged();
        }
    }

    public void Clear()
    {
        _messages.Clear();
        _isLoading = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        if (_disposed) return;

        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public ValueTask DisposeAsync()
    {
        _disposed = true;
        Console.WriteLine($"🗑️ [ChatStateService] Disposed");
        return ValueTask.CompletedTask;
    }
}
