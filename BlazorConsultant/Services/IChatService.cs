using BlazorConsultant.Models;

namespace BlazorConsultant.Services;

/// <summary>
/// Chat service interface for Python API communication.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Send chat message and receive complete response (non-streaming).
    /// Frontend simulates streaming for better UX.
    /// </summary>
    /// <param name="message">User message</param>
    /// <param name="customInstructions">Optional custom instructions</param>
    /// <param name="model">OpenAI model to use</param>
    /// <param name="attachments">File attachments</param>
    /// <param name="practitionerMode">Use practitioner mode</param>
    /// <param name="reasoningEffort">GPT-5 reasoning effort (minimal/low/medium/high)</param>
    /// <param name="verbosity">GPT-5 response verbosity (low/medium/high)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete chat response with tokens and cost</returns>
    Task<ChatResponse> SendChatAsync(
        string message,
        string? customInstructions = null,
        string? model = null,
        List<FileAttachment>? attachments = null,
        bool practitionerMode = false,
        string? reasoningEffort = null,
        string? verbosity = null,
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
