using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.JSInterop;

namespace BlazorConsultant.Services;

/// <summary>
/// SSE Stream Manager - Bridge between JavaScript EventSource and Blazor
///
/// Manages Server-Sent Events (SSE) streaming via JavaScript EventSource API.
/// Provides DotNetObjectReference callbacks for JavaScript to call.
/// Implements IAsyncDisposable for proper cleanup.
/// </summary>
public class SseStreamManager : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<SseStreamManager> _logger;
    private DotNetObjectReference<SseStreamManager>? _dotNetRef;
    private Channel<string>? _tokenChannel;
    private string? _currentRequestId;
    private TaskCompletionSource? _streamCompletionSource;
    private int _disposed; // 0 = not disposed, 1 = disposed (thread-safe with Interlocked)

    public SseStreamManager(IJSRuntime jsRuntime, ILogger<SseStreamManager> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    /// <summary>
    /// Stream chat response via JavaScript EventSource
    /// </summary>
    /// <param name="url">SSE endpoint URL</param>
    /// <param name="cancellationToken">Cancellation token to stop streaming</param>
    /// <returns>Async enumerable of tokens</returns>
    public async IAsyncEnumerable<string> StreamAsync(
        string url,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            throw new ObjectDisposedException(nameof(SseStreamManager));

        // Generate unique request ID
        _currentRequestId = Guid.NewGuid().ToString("N");

        _logger.LogInformation($"[SSE] Starting stream {_currentRequestId}");

        // Create channel for streaming tokens
        _tokenChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        // Create completion source to track stream lifecycle
        _streamCompletionSource = new TaskCompletionSource();

        // Create DotNetObjectReference for JavaScript callbacks
        _dotNetRef = DotNetObjectReference.Create(this);

        // Note: Cannot use try-catch with yield return (C# limitation)
        // Only try-finally is allowed. Exception handling must be done by caller.
        try
        {
            // Start JavaScript EventSource
            await _jsRuntime.InvokeVoidAsync("sseClient.connect", url, _dotNetRef, _currentRequestId);
        }
        catch (JSDisconnectedException)
        {
            // SignalR disconnected before stream started - cleanup and exit
            _logger.LogInformation($"[SSE] SignalR circuit disconnected before stream start");
            await CleanupStreamAsync();
            yield break;
        }
        catch (Exception ex)
        {
            // Failed to start stream - cleanup and rethrow
            _logger.LogError(ex, $"[SSE] Failed to start stream: {ex.Message}");
            await CleanupStreamAsync();
            throw;
        }

        // Stream tokens - can only use try-finally (no catch) with yield return
        try
        {
            // Read tokens from channel until stream completes or cancellation
            await foreach (var token in _tokenChannel.Reader.ReadAllAsync(cancellationToken))
            {
                if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
                {
                    _logger.LogWarning($"[SSE] Component disposed during streaming, stopping");
                    break;
                }

                yield return token;
            }

            // Wait for stream to complete gracefully (or cancellation)
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5)); // 5 second grace period

            try
            {
                await _streamCompletionSource.Task.WaitAsync(cts.Token);
                _logger.LogInformation($"[SSE] Stream {_currentRequestId} completed successfully");
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"[SSE] Stream {_currentRequestId} cancelled by user");
                }
                else
                {
                    _logger.LogWarning($"[SSE] Stream {_currentRequestId} completion timeout");
                }
            }
        }
        finally
        {
            // Cleanup JavaScript EventSource
            await CleanupStreamAsync();
        }
    }

    /// <summary>
    /// JavaScript callback: Connection established
    /// </summary>
    [JSInvokable]
    public Task OnConnected()
    {
        if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            return Task.CompletedTask;

        _logger.LogInformation($"[SSE] EventSource connected for request {_currentRequestId}");
        return Task.CompletedTask;
    }

    /// <summary>
    /// JavaScript callback: Token received from SSE stream
    /// </summary>
    /// <param name="token">Token string from backend</param>
    [JSInvokable]
    public async Task OnTokenReceived(string token)
    {
        if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1 || _tokenChannel == null)
            return;

        try
        {
            await _tokenChannel.Writer.WriteAsync(token);
        }
        catch (ChannelClosedException)
        {
            _logger.LogWarning($"[SSE] Attempted to write token to closed channel");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[SSE] Error writing token to channel: {ex.Message}");
        }
    }

    /// <summary>
    /// JavaScript callback: Stream completed successfully
    /// </summary>
    [JSInvokable]
    public Task OnStreamComplete()
    {
        if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            return Task.CompletedTask;

        _logger.LogInformation($"[SSE] Stream {_currentRequestId} received completion signal");

        // Close the token channel (no more tokens will arrive)
        _tokenChannel?.Writer.Complete();

        // Mark stream as completed
        _streamCompletionSource?.TrySetResult();

        return Task.CompletedTask;
    }

    /// <summary>
    /// JavaScript callback: Stream error occurred
    /// </summary>
    /// <param name="errorMessage">Error message from JavaScript</param>
    [JSInvokable]
    public Task OnStreamError(string errorMessage)
    {
        if (Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
            return Task.CompletedTask;

        _logger.LogError($"[SSE] Stream {_currentRequestId} error: {errorMessage}");

        // Close the token channel with error
        // Use TryComplete to avoid exceptions if channel already completed
        _tokenChannel?.Writer.TryComplete(new IOException($"SSE error: {errorMessage}"));

        // Mark stream as failed
        _streamCompletionSource?.TrySetException(new IOException(errorMessage));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Cleanup JavaScript EventSource and resources
    /// </summary>
    private async Task CleanupStreamAsync()
    {
        if (_currentRequestId == null) return;

        try
        {
            // Tell JavaScript to close EventSource
            await _jsRuntime.InvokeVoidAsync("sseClient.disconnect", _currentRequestId);
        }
        catch (JSDisconnectedException)
        {
            // SignalR already disconnected - cleanup not needed
            _logger.LogInformation($"[SSE] SignalR disconnected, skipping JavaScript cleanup");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"[SSE] Error during JavaScript cleanup: {ex.Message}");
        }

        // Close channel if still open
        _tokenChannel?.Writer.Complete();

        // Dispose DotNetObjectReference
        _dotNetRef?.Dispose();
        _dotNetRef = null;

        _currentRequestId = null;
    }

    /// <summary>
    /// Dispose pattern implementation
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // Thread-safe disposal check - only dispose once
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        _logger.LogInformation($"[SSE] Disposing SseStreamManager");

        // Cleanup active stream
        await CleanupStreamAsync();

        // Mark stream as cancelled
        _streamCompletionSource?.TrySetCanceled();
    }
}
