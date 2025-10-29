using BlazorConsultant.Models;

namespace BlazorConsultant.Data;

/// <summary>
/// Repository interface for SystemPrompt data access operations.
/// Abstracts database access from business logic.
/// </summary>
public interface ISystemPromptRepository
{
    /// <summary>
    /// Gets all prompts, optionally filtered by active status.
    /// </summary>
    Task<IEnumerable<SystemPromptDto>> GetAllAsync(bool activeOnly = false);

    /// <summary>
    /// Gets a specific prompt by ID.
    /// </summary>
    Task<SystemPromptDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets the current default prompt.
    /// </summary>
    Task<SystemPromptDto?> GetDefaultAsync();

    /// <summary>
    /// Searches prompts by name using LIKE query.
    /// </summary>
    Task<IEnumerable<SystemPromptDto>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Creates a new prompt and returns the generated ID.
    /// </summary>
    Task<int> CreateAsync(SystemPromptDto prompt);

    /// <summary>
    /// Updates an existing prompt.
    /// </summary>
    Task<bool> UpdateAsync(SystemPromptDto prompt);

    /// <summary>
    /// Deletes a prompt by ID.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Sets a prompt as the default (unsetting all others atomically).
    /// </summary>
    Task<bool> SetDefaultAsync(int id);
}
