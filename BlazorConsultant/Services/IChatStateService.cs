using BlazorConsultant.Models;

namespace BlazorConsultant.Services;

/// <summary>
/// Chat state management service interface.
/// Maintains conversation state that persists across navigation.
/// Scoped per SignalR connection.
/// </summary>
public interface IChatStateService : IAsyncDisposable
{
    /// <summary>
    /// All messages in the current conversation.
    /// </summary>
    IReadOnlyList<ChatMessage> Messages { get; }

    /// <summary>
    /// Whether the service is currently loading a response.
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Event raised when chat state changes (new message, loading state, etc.).
    /// Subscribe to this to update UI.
    /// </summary>
    event EventHandler? OnStateChanged;

    /// <summary>
    /// Send a message and receive the AI response.
    /// </summary>
    /// <param name="message">User's message</param>
    /// <param name="attachments">Optional file attachments</param>
    /// <param name="reasoningEffort">GPT-5 reasoning effort (minimal/low/medium/high)</param>
    /// <param name="verbosity">GPT-5 response verbosity (low/medium/high)</param>
    Task SendMessageAsync(
        string message,
        List<FileAttachment>? attachments = null,
        string? reasoningEffort = null,
        string? verbosity = null);

    /// <summary>
    /// Clear all conversation history.
    /// </summary>
    void Clear();
}
