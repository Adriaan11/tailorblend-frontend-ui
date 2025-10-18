using System.Text.Json.Serialization;

namespace BlazorConsultant.Models;

/// <summary>
/// Represents a complete trace of an agent execution.
/// </summary>
public class TraceInfo
{
    [JsonPropertyName("trace_id")]
    public string TraceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("started_at")]
    public string StartedAt { get; set; } = string.Empty;

    [JsonPropertyName("ended_at")]
    public string? EndedAt { get; set; }

    [JsonPropertyName("duration_ms")]
    public int? DurationMs { get; set; }

    [JsonPropertyName("spans")]
    public List<SpanInfo> Spans { get; set; } = new();

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a single span within a trace (LLM call, tool call, etc).
/// </summary>
public class SpanInfo
{
    [JsonPropertyName("span_id")]
    public string SpanId { get; set; } = string.Empty;

    [JsonPropertyName("trace_id")]
    public string TraceId { get; set; } = string.Empty;

    [JsonPropertyName("parent_id")]
    public string? ParentId { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;  // generation, function, agent, etc.

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("started_at")]
    public string StartedAt { get; set; } = string.Empty;

    [JsonPropertyName("ended_at")]
    public string? EndedAt { get; set; }

    [JsonPropertyName("duration_ms")]
    public int? DurationMs { get; set; }

    [JsonPropertyName("data")]
    public SpanData Data { get; set; } = new();
}

/// <summary>
/// Type-specific data for a span.
/// </summary>
public class SpanData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("input")]
    public object? Input { get; set; }

    [JsonPropertyName("output")]
    public object? Output { get; set; }

    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("mcp_data")]
    public object? McpData { get; set; }

    [JsonPropertyName("handoffs")]
    public List<string>? Handoffs { get; set; }

    [JsonPropertyName("tools")]
    public List<string>? Tools { get; set; }

    [JsonPropertyName("output_type")]
    public string? OutputType { get; set; }
}

/// <summary>
/// Token usage information for a span.
/// </summary>
public class TokenUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

/// <summary>
/// Response from /api/session/{session_id}/traces endpoint.
/// </summary>
public class TracesResponse
{
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("traces")]
    public List<TraceInfo> Traces { get; set; } = new();

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
