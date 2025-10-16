using BlazorConsultant.Models;

namespace BlazorConsultant.Services;

/// <summary>
/// Instruction service interface for managing agent instructions.
/// </summary>
public interface IInstructionService
{
    /// <summary>
    /// Get instruction sections from Python API.
    /// </summary>
    Task<List<InstructionSection>> GetSectionsAsync();

    /// <summary>
    /// Get default instruction sections.
    /// </summary>
    Task<List<InstructionSection>> GetDefaultSectionsAsync();

    /// <summary>
    /// Update instruction sections on Python API.
    /// </summary>
    Task<bool> UpdateSectionsAsync(List<InstructionSection> sections);

    /// <summary>
    /// Update raw instruction text (bypasses sections).
    /// </summary>
    Task<bool> UpdateRawInstructionsAsync(string rawInstructions);

    /// <summary>
    /// Get full instruction text.
    /// </summary>
    Task<string> GetFullTextAsync();
}
