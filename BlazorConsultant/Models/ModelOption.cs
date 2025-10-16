namespace BlazorConsultant.Models;

/// <summary>
/// Represents an OpenAI model option for the AI consultant.
/// </summary>
public class ModelOption
{
    /// <summary>
    /// Unique identifier for the model (used in UI)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI model identifier (sent to API)
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the model
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
