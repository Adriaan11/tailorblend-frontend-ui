using BlazorConsultant.Models;
using BlazorConsultant.Configuration;

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
    private readonly ILogger<ChatStateService> _logger;
    private readonly List<ChatMessage> _messages = new();
    private bool _isLoading;
    private bool _disposed;

    public ChatStateService(
        IChatService chatService,
        ISessionService sessionService,
        ILogger<ChatStateService> logger)
    {
        _chatService = chatService;
        _sessionService = sessionService;
        _logger = logger;
    }

    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

    public bool IsLoading => _isLoading;

    public event EventHandler? OnStateChanged;

    public async Task SendMessageAsync(
        string message,
        List<FileAttachment>? attachments = null,
        string? customInstructions = null,
        string? reasoningEffort = null,
        string? verbosity = null,
        CancellationToken cancellationToken = default)
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
            _logger.LogInformation("Sending chat message with {AttachmentCount} attachments", attachments?.Count ?? 0);

            if (!string.IsNullOrEmpty(customInstructions))
            {
                _logger.LogInformation("✅ ChatStateService: Passing custom instructions to ChatService (length: {Length})",
                    customInstructions.Length);
            }

            var response = await _chatService.SendChatAsync(
                message,
                customInstructions: customInstructions,
                model: _sessionService.CurrentModel,
                attachments: attachments,
                practitionerMode: false,
                reasoningEffort: reasoningEffort,
                verbosity: verbosity,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);

            _logger.LogInformation("Received chat response: {Length} chars", response.Response.Length);

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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // User cancelled - log and don't add error message
            _logger.LogInformation("Chat message cancelled by user");
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex, "Chat message timed out");

            var errorMessage = new ChatMessage
            {
                Role = "assistant",
                Content = $"⚠️ Request timed out. The server is taking too long to respond. Please try again.",
                Timestamp = DateTime.Now
            };
            _messages.Add(errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat request failed: {Message}", ex.Message);

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
        _logger.LogInformation("ChatStateService disposed");
        return ValueTask.CompletedTask;
    }
}
