using BlazorConsultant.Data;
using BlazorConsultant.Models;

namespace BlazorConsultant.Services;

/// <summary>
/// Service implementation for prompt management with validation and business logic.
/// </summary>
public class PromptManagementService : IPromptManagementService
{
    private readonly ISystemPromptRepository _repository;
    private readonly IInstructionService _instructionService;
    private readonly ILogger<PromptManagementService> _logger;

    public PromptManagementService(
        ISystemPromptRepository repository,
        IInstructionService instructionService,
        ILogger<PromptManagementService> logger)
    {
        _repository = repository;
        _instructionService = instructionService;
        _logger = logger;
    }

    public async Task<List<SystemPromptDto>> GetAllPromptsAsync(bool activeOnly = false)
    {
        var prompts = await _repository.GetAllAsync(activeOnly);
        return prompts.ToList();
    }

    public async Task<SystemPromptDto?> GetPromptByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<SystemPromptDto?> GetDefaultPromptAsync()
    {
        return await _repository.GetDefaultAsync();
    }

    public async Task<List<SystemPromptDto>> SearchPromptsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllPromptsAsync();
        }

        var results = await _repository.SearchByNameAsync(searchTerm);
        return results.ToList();
    }

    public async Task<(bool Success, int? PromptId, string? Error)> CreatePromptAsync(SystemPromptDto prompt)
    {
        // Validation
        var validationError = ValidatePrompt(prompt);
        if (validationError != null)
        {
            return (false, null, validationError);
        }

        try
        {
            var id = await _repository.CreateAsync(prompt);
            _logger.LogInformation("Successfully created prompt {PromptId}: {PromptName}", id, prompt.Name);
            return (true, id, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create prompt: {PromptName}", prompt.Name);
            return (false, null, $"Database error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> UpdatePromptAsync(SystemPromptDto prompt)
    {
        // Validation
        if (prompt.Id <= 0)
        {
            return (false, "Invalid prompt ID");
        }

        var validationError = ValidatePrompt(prompt);
        if (validationError != null)
        {
            return (false, validationError);
        }

        try
        {
            var success = await _repository.UpdateAsync(prompt);
            if (success)
            {
                _logger.LogInformation("Successfully updated prompt {PromptId}: {PromptName}", prompt.Id, prompt.Name);
                return (true, null);
            }

            return (false, "Prompt not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update prompt {PromptId}", prompt.Id);
            return (false, $"Database error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> DeletePromptAsync(int id)
    {
        if (id <= 0)
        {
            return (false, "Invalid prompt ID");
        }

        try
        {
            // Check if it's the default prompt
            var prompt = await _repository.GetByIdAsync(id);
            if (prompt == null)
            {
                return (false, "Prompt not found");
            }

            if (prompt.IsDefault)
            {
                _logger.LogWarning("Attempt to delete default prompt {PromptId}", id);
                return (false, "Cannot delete the default prompt. Please set another prompt as default first.");
            }

            var success = await _repository.DeleteAsync(id);
            if (success)
            {
                _logger.LogInformation("Successfully deleted prompt {PromptId}", id);
                return (true, null);
            }

            return (false, "Prompt not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete prompt {PromptId}", id);
            return (false, $"Database error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> SetDefaultPromptAsync(int id)
    {
        if (id <= 0)
        {
            return (false, "Invalid prompt ID");
        }

        try
        {
            // Verify prompt exists and is active
            var prompt = await _repository.GetByIdAsync(id);
            if (prompt == null)
            {
                return (false, "Prompt not found");
            }

            if (!prompt.IsActive)
            {
                return (false, "Cannot set an inactive prompt as default. Please activate it first.");
            }

            var success = await _repository.SetDefaultAsync(id);
            if (success)
            {
                _logger.LogInformation("Successfully set prompt {PromptId} as default", id);
                return (true, null);
            }

            return (false, "Failed to set default prompt");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set default prompt {PromptId}", id);
            return (false, $"Database error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> ImportInstructionsTxtAsync()
    {
        try
        {
            _logger.LogInformation("Importing instructions.txt from backend");

            // Fetch from backend API
            var instructionsText = await _instructionService.GetFullTextAsync();

            if (string.IsNullOrWhiteSpace(instructionsText))
            {
                return (false, "Failed to fetch instructions from backend. The response was empty.");
            }

            // Create new prompt
            var prompt = new SystemPromptDto
            {
                Name = "Default Instructions (Imported)",
                Description = "Auto-imported from backend instructions.txt on first run",
                Content = instructionsText,
                IsActive = true,
                IsDefault = true
            };

            var (success, promptId, error) = await CreatePromptAsync(prompt);

            if (success)
            {
                _logger.LogInformation("Successfully imported instructions.txt as prompt {PromptId}", promptId);
                return (true, null);
            }

            return (false, error ?? "Unknown error during import");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import instructions.txt");
            return (false, $"Import failed: {ex.Message}");
        }
    }

    public async Task<bool> HasPromptsAsync()
    {
        var prompts = await _repository.GetAllAsync();
        return prompts.Any();
    }

    // Private validation method
    private string? ValidatePrompt(SystemPromptDto prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt.Name))
        {
            return "Name is required";
        }

        if (prompt.Name.Length > 100)
        {
            return "Name cannot exceed 100 characters";
        }

        if (string.IsNullOrWhiteSpace(prompt.Content))
        {
            return "Content is required";
        }

        if (prompt.Content.Length > 100000)
        {
            return "Content exceeds maximum length (100,000 characters)";
        }

        if (prompt.Content.Length < 10)
        {
            return "Content must be at least 10 characters long";
        }

        if (prompt.Description != null && prompt.Description.Length > 500)
        {
            return "Description cannot exceed 500 characters";
        }

        return null; // Valid
    }
}
