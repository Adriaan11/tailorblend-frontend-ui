using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorConsultant.Models;

/// <summary>
/// Model configuration settings for controlling LLM behavior.
/// Maps to Python backend's ModelSettingsRequest model.
/// All fields are optional - null values use model defaults.
/// </summary>
public class ModelSettings
{
    /// <summary>
    /// Controls randomness/creativity (0.0 = deterministic, 2.0 = very creative).
    /// Default: null (model default ~0.7)
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>
    /// Nucleus sampling threshold (0.0-1.0). Alternative to temperature.
    /// Lower values = more focused, higher values = more diverse.
    /// Default: null (model default ~1.0)
    /// </summary>
    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }

    /// <summary>
    /// Penalty for token frequency (-2.0 to 2.0). Reduces repetition.
    /// Positive values decrease likelihood of repeating tokens.
    /// Default: null (no penalty)
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; set; }

    /// <summary>
    /// Penalty for token presence (-2.0 to 2.0). Encourages new topics.
    /// Positive values encourage model to talk about new topics.
    /// Default: null (no penalty)
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; set; }

    /// <summary>
    /// Maximum number of tokens to generate in response.
    /// Default: null (no limit)
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Response verbosity level: "low" (concise), "medium" (balanced), "high" (detailed).
    /// Default: null (model default)
    /// </summary>
    [JsonPropertyName("verbosity")]
    public string? Verbosity { get; set; }

    /// <summary>
    /// Tool selection strategy: "auto", "required", "none", or specific tool name.
    /// Default: null (auto)
    /// </summary>
    [JsonPropertyName("tool_choice")]
    public string? ToolChoice { get; set; }

    /// <summary>
    /// Allow multiple parallel tool calls in a single turn.
    /// Default: null (model default, typically true)
    /// </summary>
    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; set; }

    /// <summary>
    /// Context truncation strategy: "auto" or "disabled".
    /// Default: null (model default)
    /// </summary>
    [JsonPropertyName("truncation")]
    public string? Truncation { get; set; }

    /// <summary>
    /// Whether to store response for later retrieval.
    /// Default: null (model default)
    /// </summary>
    [JsonPropertyName("store")]
    public bool? Store { get; set; }

    /// <summary>
    /// Include usage information (token counts) in response.
    /// Default: null (model default)
    /// </summary>
    [JsonPropertyName("include_usage")]
    public bool? IncludeUsage { get; set; }

    /// <summary>
    /// Number of top token log probabilities to return (0-20).
    /// Default: null (no logprobs)
    /// </summary>
    [JsonPropertyName("top_logprobs")]
    public int? TopLogprobs { get; set; }

    /// <summary>
    /// Custom metadata key-value pairs for tracking.
    /// Default: null (no metadata)
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Creates a new ModelSettings instance with all default (null) values.
    /// </summary>
    public ModelSettings()
    {
        // All properties initialized to null by default
    }

    /// <summary>
    /// Checks if any settings have been modified from defaults.
    /// </summary>
    /// <returns>True if any non-null values are set</returns>
    public bool HasCustomSettings()
    {
        return Temperature.HasValue
            || TopP.HasValue
            || FrequencyPenalty.HasValue
            || PresencePenalty.HasValue
            || MaxTokens.HasValue
            || !string.IsNullOrEmpty(Verbosity)
            || !string.IsNullOrEmpty(ToolChoice)
            || ParallelToolCalls.HasValue
            || !string.IsNullOrEmpty(Truncation)
            || Store.HasValue
            || IncludeUsage.HasValue
            || TopLogprobs.HasValue
            || (Metadata != null && Metadata.Count > 0);
    }

    /// <summary>
    /// Resets all settings to defaults (null values).
    /// </summary>
    public void ResetToDefaults()
    {
        Temperature = null;
        TopP = null;
        FrequencyPenalty = null;
        PresencePenalty = null;
        MaxTokens = null;
        Verbosity = null;
        ToolChoice = null;
        ParallelToolCalls = null;
        Truncation = null;
        Store = null;
        IncludeUsage = null;
        TopLogprobs = null;
        Metadata = null;
    }
}
