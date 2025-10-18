using BlazorConsultant.Models;

namespace BlazorConsultant.Services;

/// <summary>
/// Chat service interface for Python API communication.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Stream chat response from Python API using SSE (HttpClient-based).
    /// NOTE: This method is ONLY for POST scenarios (attachments, practitioner mode).
    /// For text-only GET requests, use SseStreamManager instead.
    /// </summary>
    /// <param name="message">User message</param>
    /// <param name="customInstructions">Optional custom instructions</param>
    /// <param name="model">OpenAI model to use</param>
    /// <param name="attachments">File attachments (requires POST)</param>
    /// <param name="practitionerMode">Use practitioner mode (requires POST)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of response tokens</returns>
    IAsyncEnumerable<string> StreamChatAsync(
        string message,
        string? customInstructions = null,
        string? model = null,
        List<FileAttachment>? attachments = null,
        bool practitionerMode = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get session statistics from Python API.
    /// </summary>
    /// <returns>Session statistics including tokens and cost</returns>
    Task<SessionStats> GetSessionStatsAsync();

    /// <summary>
    /// Reset session conversation state.
    /// </summary>
    Task ResetSessionAsync();
}
