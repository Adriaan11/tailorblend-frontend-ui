using BlazorConsultant.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorConsultant.Services;

/// <summary>
/// Chat state management service implementation.
/// Maintains conversation state and handles streaming across navigation.
/// Scoped per SignalR connection.
/// </summary>
public class ChatStateService : IChatStateService
{
    private readonly IChatService _chatService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISessionService _sessionService;
    private readonly IConfiguration _configuration;
    private readonly List<ChatMessage> _messages = new();
    private ChatMessage? _streamingMessage;
    private bool _isStreaming;
    private Task? _streamingTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _disposed;

    public ChatStateService(
        IChatService chatService,
        IServiceProvider serviceProvider,
        ISessionService sessionService,
        IConfiguration configuration)
    {
        _chatService = chatService;
        _serviceProvider = serviceProvider;
        _sessionService = sessionService;
        _configuration = configuration;
    }

    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

    public ChatMessage? StreamingMessage => _streamingMessage;

    public bool IsStreaming => _isStreaming;

    public event EventHandler? OnStateChanged;

    public Task SendMessageAsync(string message, List<FileAttachment>? attachments = null)
    {
        if (string.IsNullOrWhiteSpace(message) || _isStreaming)
            return Task.CompletedTask;

        // Create cancellation token for this streaming operation
        _cancellationTokenSource?.Cancel();  // Cancel any existing stream
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        // Start streaming in background with task tracking
        // This allows the UI to remain responsive and navigate during streaming
        _streamingTask = Task.Run(async () =>
        {
            try
            {
                await StreamMessageInternalAsync(message, attachments, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // User cancelled - add cancellation message
                Console.WriteLine($"⏹️ [ChatStateService] Stream cancelled by user");

                var cancelMessage = new ChatMessage
                {
                    Role = "assistant",
                    Content = "⏹️ Generation stopped by user.",
                    Timestamp = DateTime.Now
                };
                _messages.Add(cancelMessage);
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                // Log unhandled exceptions
                Console.WriteLine($"❌ [ChatStateService] Unhandled exception in streaming task: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        });

        return Task.CompletedTask;
    }

    private async Task StreamMessageInternalAsync(string message, List<FileAttachment>? attachments, CancellationToken cancellationToken, int retryAttempt = 0)
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
            var hasAttachments = attachments != null && attachments.Count > 0;

            if (hasAttachments)
            {
                // File attachments require POST - use HttpClient fallback
                Console.WriteLine($"📎 [ChatStateService] Using HttpClient for file attachments");

                await foreach (var token in _chatService.StreamChatAsync(
                    message,
                    customInstructions: null,
                    model: _sessionService.CurrentModel,
                    attachments: attachments,
                    practitionerMode: false,
                    cancellationToken: cancellationToken))
                {
                    if (_disposed) break;

                    _streamingMessage.Content += token;

                    if (_disposed) return;
                    NotifyStateChanged();
                }
            }
            else
            {
                // Text-only messages use EventSource (GET)
                var pythonApiUrl = _configuration["PythonApi:BaseUrl"] ?? "http://localhost:5000";

                // Build GET request URL
                var queryParams = new Dictionary<string, string>
                {
                    { "message", message },
                    { "session_id", _sessionService.SessionId },
                    { "model", _sessionService.CurrentModel }
                };

                var queryString = string.Join("&", queryParams.Select(kvp =>
                    $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

                var sseUrl = $"{pythonApiUrl}/api/chat/stream?{queryString}";

                // Create SseStreamManager instance (scoped to this streaming operation)
                // Use 'await using' to ensure proper disposal of DotNetObjectReference
                await using var sseManager = _serviceProvider.GetRequiredService<SseStreamManager>();

                // Stream response via EventSource
                await foreach (var token in sseManager.StreamAsync(sseUrl, cancellationToken))
                {
                    if (_disposed) break;

                    _streamingMessage.Content += token;

                    if (_disposed) return;
                    NotifyStateChanged();
                }
            }

            // Finalize message
            _streamingMessage.IsStreaming = false;
            _messages.Add(_streamingMessage);
            _streamingMessage = null;

            // Increment message counter
            _sessionService.IncrementMessageCount();
        }
        catch (OperationCanceledException)
        {
            // User cancelled - propagate upward
            _streamingMessage = null;
            throw;
        }
        catch (Exception ex) when (retryAttempt < 1 && !cancellationToken.IsCancellationRequested)
        {
            // Auto-retry once for transient errors
            Console.WriteLine($"⚠️ [ChatStateService] Stream failed (attempt {retryAttempt + 1}/2), retrying in 1 second: {ex.Message}");

            _streamingMessage = null;

            // Exponential backoff
            await Task.Delay(1000 * (retryAttempt + 1), cancellationToken);

            // Retry
            await StreamMessageInternalAsync(message, attachments, cancellationToken, retryAttempt + 1);
        }
        catch (Exception ex)
        {
            // Final error - show to user
            Console.WriteLine($"❌ [ChatStateService] Stream failed after retries: {ex.Message}");

            var errorMessage = new ChatMessage
            {
                Role = "assistant",
                Content = $"⚠️ Sorry, we're having trouble connecting. Please try again.",
                Timestamp = DateTime.Now
            };
            _messages.Add(errorMessage);
            _streamingMessage = null;
        }
        finally
        {
            _isStreaming = false;
            // NotifyStateChanged has its own disposal check
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

    public void CancelStreaming()
    {
        Console.WriteLine($"🛑 [ChatStateService] CancelStreaming called");
        _cancellationTokenSource?.Cancel();
    }

    private void NotifyStateChanged()
    {
        if (_disposed) return;

        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        Console.WriteLine($"🗑️ [ChatStateService] Disposing service");

        // Cancel any active streaming
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();

        // Wait for streaming task to complete
        if (_streamingTask != null)
        {
            try
            {
                await _streamingTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ [ChatStateService] Error during disposal: {ex.Message}");
            }
        }
    }
}
