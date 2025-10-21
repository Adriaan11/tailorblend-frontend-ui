using BlazorConsultant.Models;

namespace BlazorConsultant.Services;

/// <summary>
/// Prompt evaluation service interface for testing and improving prompts.
/// </summary>
public interface IPromptEvaluationService
{
    /// <summary>
    /// Evaluate a prompt against multiple test cases.
    /// </summary>
    /// <param name="request">Evaluation request with prompt and test cases</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Evaluation results with scores and feedback</returns>
    Task<PromptEvaluationResponse> EvaluatePromptAsync(
        PromptEvaluationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get default test cases for blend assistant evaluation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of default test cases</returns>
    Task<List<EvaluationTestCase>> GetDefaultTestCasesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Save a prompt version for later comparison.
    /// </summary>
    /// <param name="version">Prompt version to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if save succeeded</returns>
    Task<bool> SavePromptVersionAsync(
        PromptVersion version,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all saved prompt versions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of saved prompt versions</returns>
    Task<List<PromptVersion>> GetSavedVersionsAsync(
        CancellationToken cancellationToken = default);
}
