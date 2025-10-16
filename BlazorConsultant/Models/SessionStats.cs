using System.Text.Json.Serialization;

namespace BlazorConsultant.Models;

/// <summary>
/// Session statistics including token usage and cost.
/// </summary>
public class SessionStats
{
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("message_count")]
    public int MessageCount { get; set; }

    [JsonPropertyName("total_input_tokens")]
    public int TotalInputTokens { get; set; }

    [JsonPropertyName("total_output_tokens")]
    public int TotalOutputTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonPropertyName("cost_zar")]
    public double CostZar { get; set; }

    [JsonPropertyName("cost_formatted")]
    public string CostFormatted { get; set; } = "R0.00";
}
