namespace BlazorConsultant.Models;

/// <summary>
/// Data transfer object for system prompts stored in SQL Server.
/// Maps directly to the SystemPrompts table.
/// </summary>
public class SystemPromptDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // UI helper properties
    public string ContentPreview => Content.Length > 100
        ? Content.Substring(0, 100) + "..."
        : Content;

    public string FormattedCreatedAt => CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

    public string FormattedUpdatedAt => UpdatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
}
