using System.Text.Json.Serialization;

namespace BlazorConsultant.Models;

/// <summary>
/// A single test case for evaluating prompts
/// </summary>
public class EvaluationTestCase
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("user_message")]
    public string UserMessage { get; set; } = "";

    [JsonPropertyName("expected_characteristics")]
    public List<string> ExpectedCharacteristics { get; set; } = new();

    [JsonPropertyName("avoid_characteristics")]
    public List<string> AvoidCharacteristics { get; set; } = new();
}

/// <summary>
/// Request to evaluate a prompt against test cases
/// </summary>
public class PromptEvaluationRequest
{
    [JsonPropertyName("prompt_text")]
    public string PromptText { get; set; } = "";

    [JsonPropertyName("test_cases")]
    public List<EvaluationTestCase> TestCases { get; set; } = new();

    [JsonPropertyName("model")]
    public string Model { get; set; } = "gpt-5-mini-2025-01-24";

    [JsonPropertyName("evaluation_criteria")]
    public List<string> EvaluationCriteria { get; set; } = new()
    {
        "accuracy",
        "helpfulness",
        "safety",
        "tone",
        "completeness"
    };
}

/// <summary>
/// Evaluation result for a single test case
/// </summary>
public class TestCaseResult
{
    [JsonPropertyName("test_case_id")]
    public string TestCaseId { get; set; } = "";

    [JsonPropertyName("test_case_name")]
    public string TestCaseName { get; set; } = "";

    [JsonPropertyName("response")]
    public string Response { get; set; } = "";

    [JsonPropertyName("score")]
    public double Score { get; set; } // 0-100

    [JsonPropertyName("feedback")]
    public string Feedback { get; set; } = "";

    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();

    [JsonPropertyName("weaknesses")]
    public List<string> Weaknesses { get; set; } = new();

    [JsonPropertyName("suggestions")]
    public List<string> Suggestions { get; set; } = new();

    [JsonPropertyName("tokens")]
    public TokenInfo? Tokens { get; set; }
}

/// <summary>
/// Complete evaluation response from backend
/// </summary>
public class PromptEvaluationResponse
{
    [JsonPropertyName("overall_score")]
    public double OverallScore { get; set; } // 0-100

    [JsonPropertyName("test_results")]
    public List<TestCaseResult> TestResults { get; set; } = new();

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";

    [JsonPropertyName("recommendations")]
    public List<string> Recommendations { get; set; } = new();

    [JsonPropertyName("total_tokens")]
    public TokenInfo? TotalTokens { get; set; }

    [JsonPropertyName("cost_zar")]
    public decimal CostZar { get; set; }

    [JsonPropertyName("evaluation_time_seconds")]
    public double EvaluationTimeSeconds { get; set; }
}

/// <summary>
/// Saved prompt version for comparison
/// </summary>
public class PromptVersion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string PromptText { get; set; } = "";
    public double? Score { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
