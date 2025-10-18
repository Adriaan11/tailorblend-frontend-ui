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
    private readonly ISessionService _sessionService;
    private readonly List<ChatMessage> _messages = new();
    private ChatMessage? _streamingMessage;
    private bool _isStreaming;
    private Task? _streamingTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _disposed;

    public ChatStateService(
        IChatService chatService,
        ISessionService sessionService)
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
            // Fetch FULL response from backend (unified code path - no more dual implementation!)
            Console.WriteLine($"⏳ [ChatStateService] Fetching complete response...");

            var response = await _chatService.SendChatAsync(
                message,
                customInstructions: null,
                model: _sessionService.CurrentModel,
                attachments: attachments,
                practitionerMode: false,
                cancellationToken: cancellationToken
            );

            Console.WriteLine($"✅ [ChatStateService] Received {response.Response.Length} chars, starting simulation");

            // Simulate streaming with gradual reveal
            using var simulator = new StreamSimulator();
            simulator.OnTokenRevealed += () =>
            {
                if (_disposed || _streamingMessage == null)
                    return;

                _streamingMessage.Content = simulator.CurrentText;
                NotifyStateChanged();
            };

            // Start simulation (20ms = ~50 chars/sec, feels natural)
            simulator.StartSimulation(response.Response, intervalMs: 20);

            // Wait for simulation to complete or cancellation
            while (!simulator.IsComplete && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                // User cancelled during simulation - show all remaining text instantly
                simulator.CompleteInstantly();
            }

            // Finalize message
            _streamingMessage.IsStreaming = false;
            _messages.Add(_streamingMessage);
            _streamingMessage = null;

            // Increment message counter
            _sessionService.IncrementMessageCount();

            Console.WriteLine($"✅ [ChatStateService] Simulation complete");
        }
        catch (OperationCanceledException)
        {
            // User cancelled during HTTP request
            Console.WriteLine($"⏹️ [ChatStateService] Request cancelled by user");

            var cancelMessage = new ChatMessage
            {
                Role = "assistant",
                Content = "⏹️ Generation stopped by user.",
                Timestamp = DateTime.Now
            };
            _messages.Add(cancelMessage);
            _streamingMessage = null;
            throw;
        }
        catch (Exception ex) when (retryAttempt < 1 && !cancellationToken.IsCancellationRequested)
        {
            // Auto-retry once for transient errors (much simpler now - retry entire request!)
            Console.WriteLine($"⚠️ [ChatStateService] Request failed (attempt {retryAttempt + 1}/2), retrying: {ex.Message}");

            _streamingMessage = null;
            await Task.Delay(1000, cancellationToken);

            // Retry entire request
            await StreamMessageInternalAsync(message, attachments, cancellationToken, retryAttempt + 1);
        }
        catch (Exception ex)
        {
            // Final error after retries
            Console.WriteLine($"❌ [ChatStateService] Request failed after retries: {ex.Message}");

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

        // Cancel HTTP request (if still fetching)
        _cancellationTokenSource?.Cancel();

        // Note: If simulation already started, cancellation token
        // will trigger CompleteInstantly() in StreamMessageInternalAsync
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
