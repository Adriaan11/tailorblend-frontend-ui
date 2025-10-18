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
    /// Current message being streamed from the AI (null if not streaming).
    /// </summary>
    ChatMessage? StreamingMessage { get; }

    /// <summary>
    /// Whether the AI is currently streaming a response.
    /// </summary>
    bool IsStreaming { get; }

    /// <summary>
    /// Event raised when chat state changes (new message, streaming token, etc.).
    /// Subscribe to this to update UI.
    /// </summary>
    event EventHandler? OnStateChanged;

    /// <summary>
    /// Send a message and start streaming the AI response.
    /// </summary>
    /// <param name="message">User's message</param>
    /// <param name="attachments">Optional file attachments</param>
    Task SendMessageAsync(string message, List<FileAttachment>? attachments = null);

    /// <summary>
    /// Clear all conversation history.
    /// </summary>
    void Clear();

    /// <summary>
    /// Cancel the current streaming operation.
    /// </summary>
    void CancelStreaming();
}
