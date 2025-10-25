using BlazorConsultant.Models;
using BlazorConsultant.Configuration;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace BlazorConsultant.Services;

/// <summary>
/// Chat service implementation - communicates with Python FastAPI backend.
/// Uses simple HTTP POST (non-streaming) - frontend simulates streaming for better UX.
/// </summary>
public class ChatService : IChatService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISessionService _sessionService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IHttpClientFactory httpClientFactory,
        ISessionService sessionService,
        ILogger<ChatService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// Send chat message and receive complete response (non-streaming).
    /// Backend accumulates full response, frontend simulates streaming.
    /// Includes retry logic for transient failures.
    /// </summary>
    public async Task<ChatResponse> SendChatAsync(
        string message,
        string? customInstructions = null,
        string? model = null,
        List<FileAttachment>? attachments = null,
        bool practitionerMode = false,
        string? reasoningEffort = null,
        string? verbosity = null,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            message,
            session_id = _sessionService.SessionId,
            custom_instructions = customInstructions,
            model = model ?? "gpt-5-mini",
            attachments = attachments ?? new List<FileAttachment>(),
            practitioner_mode = practitionerMode,
            reasoning_effort = reasoningEffort ?? "minimal",
            verbosity = verbosity ?? "medium"
        };

        _logger.LogInformation("Sending chat request for session {SessionId} with {AttachmentCount} attachments",
            _sessionService.SessionId, attachments?.Count ?? 0);

        // Retry logic for transient failures
        const int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PythonAPI");

                // Add timeout guard for chat requests
                using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
                    TimeoutPolicy.HttpRequestTimeout,
                    cancellationToken);

                var response = await client.PostAsJsonAsync("/api/chat", requestBody, timeoutCts.Token)
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var chatResponse = await response.Content.ReadFromJsonAsync<ChatResponse>(timeoutCts.Token)
                    .ConfigureAwait(false);

                _logger.LogInformation("Received chat response for session {SessionId}: {ResponseLength} chars",
                    _sessionService.SessionId, chatResponse?.Response.Length ?? 0);

                return chatResponse ?? throw new Exception("Empty response from backend");
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Timeout occurred (not parent cancellation) - don't retry
                _logger.LogWarning("Chat request timed out for session {SessionId} after {Timeout}s",
                    _sessionService.SessionId, TimeoutPolicy.HttpRequestTimeout.TotalSeconds);
                throw new TimeoutException($"Chat request timed out after {TimeoutPolicy.HttpRequestTimeout.TotalSeconds} seconds");
            }
            catch (HttpRequestException ex) when (attempt < maxRetries && IsTransientError(ex))
            {
                // Transient error - retry with exponential backoff
                int delayMs = (int)Math.Pow(2, attempt - 1) * 500; // 500ms, 1s, 2s
                _logger.LogWarning("Chat request failed with transient error (attempt {Attempt}/{MaxRetries}): {Message}. Retrying in {DelayMs}ms",
                    attempt, maxRetries, ex.Message, delayMs);

                await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                continue;
            }
            catch (Exception ex)
            {
                // Non-transient error or final attempt - throw
                if (attempt == maxRetries)
                {
                    _logger.LogError(ex, "Chat request failed after {MaxRetries} attempts for session {SessionId}",
                        maxRetries, _sessionService.SessionId);
                    throw;
                }

                // Check if it's transient before retrying
                if (IsTransientError(ex))
                {
                    int delayMs = (int)Math.Pow(2, attempt - 1) * 500;
                    _logger.LogWarning("Chat request failed with error (attempt {Attempt}/{MaxRetries}): {Message}. Retrying in {DelayMs}ms",
                        attempt, maxRetries, ex.Message, delayMs);

                    await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                throw;
            }
        }

        throw new InvalidOperationException("Chat request retry loop completed without result");
    }

    /// <summary>
    /// Determine if an exception is transient and should be retried.
    /// </summary>
    private static bool IsTransientError(Exception ex)
    {
        // Network connectivity issues
        if (ex is HttpRequestException hre)
        {
            // Connection timeout, network unreachable, etc.
            return hre.InnerException is IOException or TimeoutException;
        }

        // IOException often indicates temporary network issues
        return ex is IOException;
    }

    public async Task<SessionStats> GetSessionStatsAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        var url = $"/api/session/stats?session_id={_sessionService.SessionId}";

        _logger.LogInformation("Fetching session stats for session {SessionId}", _sessionService.SessionId);

        // Add timeout guard for stats (lower priority than chat)
        using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
            TimeoutPolicy.ComponentOperationTimeout,
            cancellationToken);

        try
        {
            var stats = await client.GetFromJsonAsync<SessionStats>(url, timeoutCts.Token)
                .ConfigureAwait(false);
            return stats ?? new SessionStats { SessionId = _sessionService.SessionId };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred
            _logger.LogWarning("Session stats request timed out for session {SessionId}", _sessionService.SessionId);
            return new SessionStats { SessionId = _sessionService.SessionId };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch session stats for session {SessionId}", _sessionService.SessionId);
            return new SessionStats { SessionId = _sessionService.SessionId };
        }
    }

    public async Task<bool> ResetSessionAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        var url = "/api/session/reset";

        _logger.LogInformation("Resetting session {SessionId}", _sessionService.SessionId);

        // Add timeout guard for session reset
        using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
            TimeoutPolicy.SessionOperationTimeout,
            cancellationToken);

        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionService.SessionId)
            });

            var response = await client.PostAsync(url, content, timeoutCts.Token)
                .ConfigureAwait(false);

            // Check if backend reset succeeded
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Session reset failed with status {StatusCode} for session {SessionId}",
                    response.StatusCode, _sessionService.SessionId);
                return false;
            }

            // Backend succeeded - also reset local session
            _sessionService.Reset();

            _logger.LogInformation("Session {SessionId} reset successfully", _sessionService.SessionId);
            return true;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred
            _logger.LogWarning("Session reset timed out for session {SessionId}", _sessionService.SessionId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset session {SessionId}", _sessionService.SessionId);
            return false;
        }
    }
}
