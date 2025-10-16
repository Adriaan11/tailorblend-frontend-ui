using System.Text.Json.Serialization;

namespace BlazorConsultant.Models;

/// <summary>
/// Request model for multi-agent blend formulation.
/// </summary>
public class MultiAgentRequest
{
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("patient_name")]
    public string? PatientName { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }

    [JsonPropertyName("sex")]
    public string? Sex { get; set; }

    [JsonPropertyName("weight")]
    public float? Weight { get; set; }

    [JsonPropertyName("health_goals")]
    public string HealthGoals { get; set; } = string.Empty;

    [JsonPropertyName("dietary_preferences")]
    public string? DietaryPreferences { get; set; }

    [JsonPropertyName("medical_conditions")]
    public string? MedicalConditions { get; set; }

    [JsonPropertyName("medications")]
    public string? Medications { get; set; }

    [JsonPropertyName("additional_info")]
    public string? AdditionalInfo { get; set; }
}

/// <summary>
/// Agent step response from streaming API.
/// </summary>
public class AgentStepResponse
{
    [JsonPropertyName("agent_name")]
    public string AgentName { get; set; } = string.Empty;

    [JsonPropertyName("step_type")]
    public string StepType { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public Dictionary<string, object>? Data { get; set; }
}
