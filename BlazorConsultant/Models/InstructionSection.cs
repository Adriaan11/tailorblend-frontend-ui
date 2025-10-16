namespace BlazorConsultant.Models;

/// <summary>
/// Represents a section of the instruction editor.
/// </summary>
public class InstructionSection
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int LineCount { get; set; } = 10;
}
