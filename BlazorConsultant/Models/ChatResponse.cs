using System.Text.Json.Serialization;

namespace BlazorConsultant.Models;

/// <summary>
/// Token usage information from backend
/// </summary>
public class TokenInfo
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

/// <summary>
/// Complete chat response (non-streaming)
/// Frontend simulates streaming for gradual reveal effect.
/// </summary>
public class ChatResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = "";

    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = "";

    [JsonPropertyName("tokens")]
    public TokenInfo Tokens { get; set; } = new();

    [JsonPropertyName("cost_zar")]
    public decimal CostZar { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("message_count")]
    public int MessageCount { get; set; }
}
