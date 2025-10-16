using System.Text.Json.Serialization;

namespace BlazorConsultant.Models;

/// <summary>
/// File attachment model for chat messages.
/// Matches the backend FileAttachment Pydantic model.
/// </summary>
public class FileAttachment
{
    /// <summary>
    /// Original filename with extension.
    /// </summary>
    [JsonPropertyName("filename")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Base64 encoded file content.
    /// </summary>
    [JsonPropertyName("base64_data")]
    public string Base64Data { get; set; } = string.Empty;

    /// <summary>
    /// MIME type (e.g., application/pdf, image/jpeg).
    /// </summary>
    [JsonPropertyName("mime_type")]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    [JsonPropertyName("file_size")]
    public int FileSize { get; set; }

    /// <summary>
    /// Get formatted file size (e.g., "2.5 MB", "150 KB").
    /// </summary>
    public string FormattedSize
    {
        get
        {
            if (FileSize < 1024)
                return $"{FileSize} B";
            if (FileSize < 1024 * 1024)
                return $"{FileSize / 1024.0:F1} KB";
            return $"{FileSize / (1024.0 * 1024.0):F1} MB";
        }
    }

    /// <summary>
    /// Get file extension (e.g., "pdf", "jpg").
    /// </summary>
    public string Extension =>
        Path.GetExtension(FileName)?.TrimStart('.').ToLower() ?? string.Empty;

    /// <summary>
    /// Check if this is an image file.
    /// </summary>
    public bool IsImage =>
        MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
}
