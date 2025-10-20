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
    /// <param name="cancellationToken">Cancellation token</param>
    Task<List<InstructionSection>> GetSectionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get default instruction sections.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<List<InstructionSection>> GetDefaultSectionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update instruction sections on Python API.
    /// </summary>
    /// <param name="sections">Instruction sections to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> UpdateSectionsAsync(List<InstructionSection> sections, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update raw instruction text (bypasses sections).
    /// </summary>
    /// <param name="rawInstructions">Raw instruction text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> UpdateRawInstructionsAsync(string rawInstructions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get full instruction text.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<string> GetFullTextAsync(CancellationToken cancellationToken = default);
}
