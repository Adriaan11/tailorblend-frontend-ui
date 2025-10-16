using BlazorConsultant.Models;

namespace BlazorConsultant.Services;

/// <summary>
/// Chat state management service implementation.
/// Maintains conversation state and handles streaming across navigation.
/// Scoped per SignalR connection.
/// </summary>
public class ChatStateService : IChatStateService
{
    private readonly IChatService _chatService;
    private readonly ISessionService _sessionService;
    private readonly List<ChatMessage> _messages = new();
    private ChatMessage? _streamingMessage;
    private bool _isStreaming;

    public ChatStateService(IChatService chatService, ISessionService sessionService)
    {
        _chatService = chatService;
        _sessionService = sessionService;
    }

    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

    public ChatMessage? StreamingMessage => _streamingMessage;

    public bool IsStreaming => _isStreaming;

    public event EventHandler? OnStateChanged;

    public Task SendMessageAsync(string message, List<FileAttachment>? attachments = null)
    {
        if (string.IsNullOrWhiteSpace(message) || _isStreaming)
            return Task.CompletedTask;

        // Start streaming in background - don't await (fire-and-forget)
        // This allows the UI to remain responsive and navigate during streaming
        _ = Task.Run(async () => await StreamMessageInternalAsync(message, attachments));

        return Task.CompletedTask;
    }

    private async Task StreamMessageInternalAsync(string message, List<FileAttachment>? attachments)
    {
        // Add user message
        var userMessage = new ChatMessage
        {
            Role = "user",
            Content = message.Trim(),
            Timestamp = DateTime.Now,
            Attachments = attachments ?? new List<FileAttachment>()
        };
        _messages.Add(userMessage);

        _isStreaming = true;

        // Initialize streaming message
        _streamingMessage = new ChatMessage
        {
            Role = "assistant",
            Content = "",
            Timestamp = DateTime.Now,
            IsStreaming = true
        };

        // Notify UI of state change
        NotifyStateChanged();

        try
        {
            // Stream response from Python API
            await foreach (var token in _chatService.StreamChatAsync(
                message,
                customInstructions: null,
                model: _sessionService.CurrentModel,
                attachments: attachments))
            {
                _streamingMessage.Content += token;
                NotifyStateChanged();
            }

            // Finalize message
            _streamingMessage.IsStreaming = false;
            _messages.Add(_streamingMessage);
            _streamingMessage = null;

            // Increment message counter
            _sessionService.IncrementMessageCount();
        }
        catch (Exception ex)
        {
            // Show error to user
            var errorMessage = new ChatMessage
            {
                Role = "assistant",
                Content = $"⚠️ Sorry, something went wrong: {ex.Message}\n\nPlease try again.",
                Timestamp = DateTime.Now
            };
            _messages.Add(errorMessage);
            _streamingMessage = null;
        }
        finally
        {
            _isStreaming = false;
            NotifyStateChanged();
        }
    }

    public void Clear()
    {
        _messages.Clear();
        _streamingMessage = null;
        _isStreaming = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
}
