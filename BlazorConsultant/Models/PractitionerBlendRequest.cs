using System.ComponentModel.DataAnnotations;

namespace BlazorConsultant.Models;

/// <summary>
/// Request model for practitioner blend creation.
/// Contains comprehensive patient information for one-shot blend generation.
/// </summary>
public class PractitionerBlendRequest
{
    /// <summary>
    /// Patient's full name
    /// </summary>
    [Required(ErrorMessage = "Patient name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Patient name must be between 2 and 100 characters")]
    public string PatientName { get; set; } = string.Empty;

    /// <summary>
    /// Practitioner's email address for blend access
    /// </summary>
    [Required(ErrorMessage = "Practitioner email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string PractitionerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Patient's age in years
    /// </summary>
    [Required(ErrorMessage = "Age is required")]
    [Range(1, 120, ErrorMessage = "Age must be between 1 and 120")]
    public int Age { get; set; }

    /// <summary>
    /// Patient's biological sex
    /// </summary>
    [Required(ErrorMessage = "Sex is required")]
    public string Sex { get; set; } = string.Empty;

    /// <summary>
    /// Patient's weight in kilograms
    /// </summary>
    [Required(ErrorMessage = "Weight is required")]
    [Range(1, 300, ErrorMessage = "Weight must be between 1 and 300 kg")]
    public decimal Weight { get; set; }

    /// <summary>
    /// Chronic medical conditions (comma-separated or free text)
    /// </summary>
    public string? ChronicConditions { get; set; }

    /// <summary>
    /// Primary health objectives or areas of focus
    /// </summary>
    public string? PrimaryHealthGoals { get; set; }

    /// <summary>
    /// Current medications with dosages (comma-separated or free text)
    /// </summary>
    public string? ChronicMedications { get; set; }

    /// <summary>
    /// Additional patient information: health goals, lifestyle, diet, allergies, etc.
    /// </summary>
    public string? AdditionalInformation { get; set; }

    /// <summary>
    /// File attachments (lab results, DNA reports, etc.)
    /// </summary>
    public List<FileAttachment> Attachments { get; set; } = new();

    /// <summary>
    /// Format as a structured message for the AI assistant
    /// </summary>
    /// <returns>Formatted patient profile</returns>
    public string ToFormattedMessage()
    {
        var message = $@"**Patient Profile:**

- **Name**: {PatientName}
- **Age**: {Age} years
- **Sex**: {Sex}
- **Weight**: {Weight} kg
- **Chronic Conditions**: {ChronicConditions ?? "None reported"}
- **Primary Goals**: {PrimaryHealthGoals ?? "Not provided"}
- **Current Medications**: {ChronicMedications ?? "None reported"}
- **Additional Information**: {AdditionalInformation ?? "Not provided"}
- **Attachments**: {(Attachments.Count > 0 ? string.Join(", ", Attachments.Select(a => a.FileName)) : "None")}

Please generate a comprehensive, evidence-based supplement blend for this patient immediately.";

        return message;
    }
}
