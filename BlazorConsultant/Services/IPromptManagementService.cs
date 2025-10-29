using BlazorConsultant.Models;

namespace BlazorConsultant.Services;

/// <summary>
/// Service interface for prompt management with business logic and validation.
/// Orchestrates repository operations and enforces business rules.
/// </summary>
public interface IPromptManagementService
{
    /// <summary>
    /// Gets all prompts, optionally filtered by active status.
    /// </summary>
    Task<List<SystemPromptDto>> GetAllPromptsAsync(bool activeOnly = false);

    /// <summary>
    /// Gets a specific prompt by ID.
    /// </summary>
    Task<SystemPromptDto?> GetPromptByIdAsync(int id);

    /// <summary>
    /// Gets the current default prompt.
    /// </summary>
    Task<SystemPromptDto?> GetDefaultPromptAsync();

    /// <summary>
    /// Searches prompts by name. Returns all if search term is empty.
    /// </summary>
    Task<List<SystemPromptDto>> SearchPromptsAsync(string searchTerm);

    /// <summary>
    /// Creates a new prompt with validation.
    /// Returns (Success, PromptId, ErrorMessage).
    /// </summary>
    Task<(bool Success, int? PromptId, string? Error)> CreatePromptAsync(SystemPromptDto prompt);

    /// <summary>
    /// Updates an existing prompt with validation.
    /// Returns (Success, ErrorMessage).
    /// </summary>
    Task<(bool Success, string? Error)> UpdatePromptAsync(SystemPromptDto prompt);

    /// <summary>
    /// Deletes a prompt by ID.
    /// Returns (Success, ErrorMessage).
    /// </summary>
    Task<(bool Success, string? Error)> DeletePromptAsync(int id);

    /// <summary>
    /// Sets a prompt as the default.
    /// Returns (Success, ErrorMessage).
    /// </summary>
    Task<(bool Success, string? Error)> SetDefaultPromptAsync(int id);

    /// <summary>
    /// Imports instructions.txt from backend API as a new prompt.
    /// Returns (Success, ErrorMessage).
    /// </summary>
    Task<(bool Success, string? Error)> ImportInstructionsTxtAsync();

    /// <summary>
    /// Checks if any prompts exist in the database.
    /// </summary>
    Task<bool> HasPromptsAsync();
}
