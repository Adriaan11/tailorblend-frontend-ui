namespace BlazorConsultant.Models;

/// <summary>
/// Represents a chat message in the conversation.
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Message role (user or assistant)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Message content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of message
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// File attachments associated with this message.
    /// Only applicable for user messages.
    /// </summary>
    public List<FileAttachment> Attachments { get; set; } = new();
}
