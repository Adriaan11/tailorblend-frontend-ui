using BlazorConsultant.Models;
using BlazorConsultant.Configuration;
using System.Text.Json;

namespace BlazorConsultant.Services;

/// <summary>
/// Prompt evaluation service - communicates with Python FastAPI backend for prompt testing.
/// </summary>
public class PromptEvaluationService : IPromptEvaluationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PromptEvaluationService> _logger;
    private readonly List<PromptVersion> _savedVersions = new(); // In-memory storage for now

    public PromptEvaluationService(
        IHttpClientFactory httpClientFactory,
        ILogger<PromptEvaluationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Evaluate a prompt against multiple test cases.
    /// </summary>
    public async Task<PromptEvaluationResponse> EvaluatePromptAsync(
        PromptEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        _logger.LogInformation("Evaluating prompt with {TestCaseCount} test cases", request.TestCases.Count);

        // Add timeout guard for evaluation (can be longer than regular chat)
        using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
            TimeoutPolicy.StreamingTimeout, // Use longer timeout for evaluation
            cancellationToken);

        try
        {
            var response = await client.PostAsJsonAsync("/api/prompt-evaluation/evaluate", request, timeoutCts.Token)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var evaluationResponse = await response.Content.ReadFromJsonAsync<PromptEvaluationResponse>(timeoutCts.Token)
                .ConfigureAwait(false);

            _logger.LogInformation("Prompt evaluation completed with overall score: {Score}",
                evaluationResponse?.OverallScore ?? 0);

            return evaluationResponse ?? throw new Exception("Empty response from backend");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Backend endpoint not implemented yet - return mock data for development
            _logger.LogWarning("Backend endpoint not available, returning mock evaluation data");
            return await GenerateMockEvaluationAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred
            _logger.LogWarning("Prompt evaluation timed out after {Timeout}s",
                TimeoutPolicy.StreamingTimeout.TotalSeconds);
            throw new TimeoutException($"Evaluation timed out after {TimeoutPolicy.StreamingTimeout.TotalSeconds} seconds");
        }
    }

    /// <summary>
    /// Get default test cases for blend assistant evaluation.
    /// </summary>
    public async Task<List<EvaluationTestCase>> GetDefaultTestCasesAsync(
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        _logger.LogInformation("Fetching default test cases");

        using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
            TimeoutPolicy.ComponentOperationTimeout,
            cancellationToken);

        try
        {
            var testCases = await client.GetFromJsonAsync<List<EvaluationTestCase>>(
                "/api/prompt-evaluation/default-test-cases", timeoutCts.Token)
                .ConfigureAwait(false);

            return testCases ?? GetFallbackTestCases();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch default test cases from backend, using fallback");
            return GetFallbackTestCases();
        }
    }

    /// <summary>
    /// Save a prompt version for later comparison (in-memory for now).
    /// </summary>
    public Task<bool> SavePromptVersionAsync(
        PromptVersion version,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Remove existing version with same ID
            _savedVersions.RemoveAll(v => v.Id == version.Id);

            // Add new version
            _savedVersions.Add(version);

            _logger.LogInformation("Saved prompt version: {Name} (Score: {Score})",
                version.Name, version.Score);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save prompt version");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Get all saved prompt versions.
    /// </summary>
    public Task<List<PromptVersion>> GetSavedVersionsAsync(
        CancellationToken cancellationToken = default)
    {
        // Return sorted by creation date (newest first)
        var sorted = _savedVersions.OrderByDescending(v => v.CreatedAt).ToList();
        return Task.FromResult(sorted);
    }

    /// <summary>
    /// Generate mock evaluation data when backend is not available.
    /// This helps with frontend development and testing.
    /// </summary>
    private async Task<PromptEvaluationResponse> GenerateMockEvaluationAsync(
        PromptEvaluationRequest request,
        CancellationToken cancellationToken)
    {
        // Simulate processing time
        await Task.Delay(2000, cancellationToken);

        var testResults = request.TestCases.Select(tc => new TestCaseResult
        {
            TestCaseId = tc.Id,
            TestCaseName = tc.Name,
            Response = $"[Mock Response] This is a simulated response to: {tc.UserMessage}",
            Score = Random.Shared.Next(60, 95),
            Feedback = "Backend endpoint not yet implemented. This is mock evaluation data.",
            Strengths = new List<string>
            {
                "Clear and professional tone",
                "Addresses user concern directly"
            },
            Weaknesses = new List<string>
            {
                "Could provide more specific recommendations",
                "May need more context gathering"
            },
            Suggestions = new List<string>
            {
                "Ask follow-up questions about symptoms",
                "Reference specific supplement ingredients"
            },
            Tokens = new TokenInfo
            {
                InputTokens = Random.Shared.Next(100, 300),
                OutputTokens = Random.Shared.Next(150, 400),
                TotalTokens = 0
            }
        }).ToList();

        // Calculate total tokens
        foreach (var result in testResults)
        {
            if (result.Tokens != null)
            {
                result.Tokens.TotalTokens = result.Tokens.InputTokens + result.Tokens.OutputTokens;
            }
        }

        var overallScore = testResults.Average(r => r.Score);

        return new PromptEvaluationResponse
        {
            OverallScore = overallScore,
            TestResults = testResults,
            Summary = $"Mock evaluation completed with {testResults.Count} test cases. Overall score: {overallScore:F1}/100. " +
                     "Note: Backend endpoint /api/prompt-evaluation/evaluate not yet implemented.",
            Recommendations = new List<string>
            {
                "Implement backend evaluation endpoint for real results",
                "Add more diverse test cases",
                "Consider edge cases and error scenarios"
            },
            TotalTokens = new TokenInfo
            {
                InputTokens = testResults.Sum(r => r.Tokens?.InputTokens ?? 0),
                OutputTokens = testResults.Sum(r => r.Tokens?.OutputTokens ?? 0),
                TotalTokens = testResults.Sum(r => r.Tokens?.TotalTokens ?? 0)
            },
            CostZar = 0.15m * testResults.Count, // Mock cost
            EvaluationTimeSeconds = 2.5
        };
    }

    /// <summary>
    /// Get fallback test cases when backend is not available.
    /// </summary>
    private List<EvaluationTestCase> GetFallbackTestCases()
    {
        return new List<EvaluationTestCase>
        {
            new EvaluationTestCase
            {
                Name = "Energy & Fatigue",
                UserMessage = "I'm always tired, even after sleeping 8 hours. What can help?",
                ExpectedCharacteristics = new List<string>
                {
                    "Ask about sleep quality",
                    "Inquire about stress levels",
                    "Suggest energy-supporting supplements",
                    "Recommend consultation if severe"
                },
                AvoidCharacteristics = new List<string>
                {
                    "Immediate diagnosis",
                    "Excessive medical jargon",
                    "Pushy sales language"
                }
            },
            new EvaluationTestCase
            {
                Name = "Stress & Anxiety",
                UserMessage = "I've been feeling really stressed and anxious lately. Can supplements help?",
                ExpectedCharacteristics = new List<string>
                {
                    "Empathetic tone",
                    "Ask about triggers",
                    "Suggest calming ingredients (magnesium, ashwagandha)",
                    "Mention lifestyle factors"
                },
                AvoidCharacteristics = new List<string>
                {
                    "Dismissive tone",
                    "Overpromising results",
                    "Replacing professional mental health support"
                }
            },
            new EvaluationTestCase
            {
                Name = "Sleep Issues",
                UserMessage = "I can't fall asleep at night. What supplements would you recommend?",
                ExpectedCharacteristics = new List<string>
                {
                    "Ask about sleep routine",
                    "Suggest melatonin or magnesium",
                    "Mention sleep hygiene",
                    "Check for contraindications"
                },
                AvoidCharacteristics = new List<string>
                {
                    "Recommending prescription alternatives",
                    "Ignoring potential underlying issues"
                }
            },
            new EvaluationTestCase
            {
                Name = "Athletic Performance",
                UserMessage = "I'm training for a marathon. What supplements can boost my performance?",
                ExpectedCharacteristics = new List<string>
                {
                    "Ask about current training level",
                    "Suggest performance ingredients (creatine, beta-alanine)",
                    "Discuss timing and dosage",
                    "Emphasize proper nutrition"
                },
                AvoidCharacteristics = new List<string>
                {
                    "Unrealistic performance claims",
                    "Ignoring safety considerations"
                }
            },
            new EvaluationTestCase
            {
                Name = "Medication Interaction Check",
                UserMessage = "I'm taking blood pressure medication. Is it safe to take supplements?",
                ExpectedCharacteristics = new List<string>
                {
                    "Request specific medication names",
                    "Cautious and safety-focused",
                    "Recommend consulting healthcare provider",
                    "Flag potential interactions"
                },
                AvoidCharacteristics = new List<string>
                {
                    "Dismissing medication concerns",
                    "Providing medical advice",
                    "Proceeding without proper safety checks"
                }
            },
            new EvaluationTestCase
            {
                Name = "Budget Constraints",
                UserMessage = "I'm interested but I'm on a tight budget. What are the most essential supplements?",
                ExpectedCharacteristics = new List<string>
                {
                    "Understanding tone",
                    "Prioritize cost-effective options",
                    "Focus on essential ingredients",
                    "Explain value proposition"
                },
                AvoidCharacteristics = new List<string>
                {
                    "Dismissive of budget concerns",
                    "Pushing expensive options",
                    "Judgmental language"
                }
            }
        };
    }
}
