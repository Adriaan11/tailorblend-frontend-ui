using BlazorConsultant.Models;

namespace BlazorConsultant.Services;

/// <summary>
/// Chat service interface for Python API communication.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Stream chat response from Python API using SSE.
    /// </summary>
    /// <param name="message">User message</param>
    /// <param name="customInstructions">Optional custom instructions</param>
    /// <param name="model">OpenAI model to use (defaults to gpt-4.1-mini-2025-04-14)</param>
    /// <param name="attachments">Optional file attachments</param>
    /// <param name="practitionerMode">Use practitioner-specific instructions if true</param>
    /// <returns>Async enumerable of response tokens</returns>
    IAsyncEnumerable<string> StreamChatAsync(
        string message,
        string? customInstructions = null,
        string? model = null,
        List<FileAttachment>? attachments = null,
        bool practitionerMode = false);

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
