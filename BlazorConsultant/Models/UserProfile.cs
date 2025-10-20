namespace BlazorConsultant.Models;

/// <summary>
/// Optional user profile data for context enrichment.
/// When filled, fields are appended to chat messages to help AI skip information-gathering questions.
/// </summary>
public class UserProfile
{
    #region Core Identity

    /// <summary>
    /// User's first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's surname
    /// </summary>
    public string? Surname { get; set; }

    /// <summary>
    /// Email address for blend registration
    /// </summary>
    public string? Email { get; set; }

    #endregion

    #region Demographics (Critical for dosing/safety)

    /// <summary>
    /// Age in years (affects dosing and safety thresholds)
    /// </summary>
    public int? Age { get; set; }

    /// <summary>
    /// Biological sex (Male/Female/Other) - impacts hormonal considerations and nutrient needs
    /// </summary>
    public string? BiologicalSex { get; set; }

    /// <summary>
    /// Height in centimeters (used for BMI and dosing calculations)
    /// </summary>
    public int? HeightCm { get; set; }

    /// <summary>
    /// Weight in kilograms (critical for accurate dosing)
    /// </summary>
    public int? WeightKg { get; set; }

    #endregion

    #region Safety Information (Highest priority for AI)

    /// <summary>
    /// Current medications (for drug-supplement interaction checking)
    /// </summary>
    public string? CurrentMedications { get; set; }

    /// <summary>
    /// Known allergies (critical safety check)
    /// </summary>
    public string? KnownAllergies { get; set; }

    /// <summary>
    /// Chronic health conditions (e.g., diabetes, hypertension, thyroid issues)
    /// </summary>
    public string? HealthConditions { get; set; }

    #endregion

    #region Lifestyle Factors (Impacts formulation)

    /// <summary>
    /// Diet type: Omnivore, Vegetarian, Vegan, Keto, High Protein, Paleo, Other
    /// </summary>
    public string? DietType { get; set; }

    /// <summary>
    /// Activity level: Sedentary, Light Activity, Moderate Exercise, Very Active, Athlete
    /// </summary>
    public string? ActivityLevel { get; set; }

    /// <summary>
    /// Smoking status: Non-smoker, Occasional, Daily
    /// </summary>
    public string? SmokingStatus { get; set; }

    /// <summary>
    /// Alcohol consumption: None, Occasional (1-2x/week), Moderate (3-5x/week), Heavy (Daily)
    /// </summary>
    public string? AlcoholConsumption { get; set; }

    #endregion

    #region Sleep & Energy (Common consultation goals)

    /// <summary>
    /// Average sleep duration in hours per night
    /// </summary>
    public decimal? AverageSleepHours { get; set; }

    /// <summary>
    /// Caffeine intake: None, 1-2 cups/day, 3-5 cups/day, 6+ cups/day
    /// </summary>
    public string? CaffeineIntake { get; set; }

    #endregion

    #region Preferences (UX enhancement)

    /// <summary>
    /// Preferred base type: Drink (zero calorie), Shake (Whey), Shake (Vegan)
    /// </summary>
    public string? PreferredBaseType { get; set; }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Check if any profile field is filled
    /// </summary>
    public bool HasAnyData()
    {
        return !string.IsNullOrWhiteSpace(FirstName)
            || !string.IsNullOrWhiteSpace(Surname)
            || !string.IsNullOrWhiteSpace(Email)
            || Age.HasValue
            || !string.IsNullOrWhiteSpace(BiologicalSex)
            || HeightCm.HasValue
            || WeightKg.HasValue
            || !string.IsNullOrWhiteSpace(CurrentMedications)
            || !string.IsNullOrWhiteSpace(KnownAllergies)
            || !string.IsNullOrWhiteSpace(HealthConditions)
            || !string.IsNullOrWhiteSpace(DietType)
            || !string.IsNullOrWhiteSpace(ActivityLevel)
            || !string.IsNullOrWhiteSpace(SmokingStatus)
            || !string.IsNullOrWhiteSpace(AlcoholConsumption)
            || AverageSleepHours.HasValue
            || !string.IsNullOrWhiteSpace(CaffeineIntake)
            || !string.IsNullOrWhiteSpace(PreferredBaseType);
    }

    /// <summary>
    /// Format profile data as structured markdown context for AI
    /// </summary>
    public string ToContextString()
    {
        if (!HasAnyData())
            return string.Empty;

        var context = new System.Text.StringBuilder();
        context.AppendLine();
        context.AppendLine("[Profile Context]");

        // Core identity
        var nameParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(FirstName)) nameParts.Add(FirstName);
        if (!string.IsNullOrWhiteSpace(Surname)) nameParts.Add(Surname);
        if (nameParts.Any())
        {
            var fullName = string.Join(" ", nameParts);
            if (!string.IsNullOrWhiteSpace(Email))
                context.AppendLine($"Name: {fullName} ({Email})");
            else
                context.AppendLine($"Name: {fullName}");
        }
        else if (!string.IsNullOrWhiteSpace(Email))
        {
            context.AppendLine($"Email: {Email}");
        }

        // Demographics (single line for compactness)
        var demographics = new List<string>();
        if (Age.HasValue) demographics.Add($"Age: {Age}");
        if (!string.IsNullOrWhiteSpace(BiologicalSex)) demographics.Add($"Sex: {BiologicalSex}");
        if (HeightCm.HasValue) demographics.Add($"Height: {HeightCm}cm");
        if (WeightKg.HasValue) demographics.Add($"Weight: {WeightKg}kg");
        if (demographics.Any())
            context.AppendLine(string.Join(" | ", demographics));

        // Lifestyle (single line)
        var lifestyle = new List<string>();
        if (!string.IsNullOrWhiteSpace(DietType)) lifestyle.Add($"Diet: {DietType}");
        if (!string.IsNullOrWhiteSpace(ActivityLevel)) lifestyle.Add($"Activity: {ActivityLevel}");
        if (lifestyle.Any())
            context.AppendLine(string.Join(" | ", lifestyle));

        // Safety (critical info)
        if (!string.IsNullOrWhiteSpace(CurrentMedications))
            context.AppendLine($"Medications: {CurrentMedications}");
        else
            context.AppendLine("Medications: None");

        if (!string.IsNullOrWhiteSpace(KnownAllergies))
            context.AppendLine($"Allergies: {KnownAllergies}");
        else
            context.AppendLine("Allergies: None");

        if (!string.IsNullOrWhiteSpace(HealthConditions))
            context.AppendLine($"Health conditions: {HealthConditions}");

        // Sleep & energy (single line)
        var sleepEnergy = new List<string>();
        if (AverageSleepHours.HasValue) sleepEnergy.Add($"Sleep: {AverageSleepHours} hours/night");
        if (!string.IsNullOrWhiteSpace(CaffeineIntake)) sleepEnergy.Add($"Caffeine: {CaffeineIntake}");
        if (sleepEnergy.Any())
            context.AppendLine(string.Join(" | ", sleepEnergy));

        // Habits (single line)
        var habits = new List<string>();
        if (!string.IsNullOrWhiteSpace(SmokingStatus)) habits.Add($"Smoking: {SmokingStatus}");
        if (!string.IsNullOrWhiteSpace(AlcoholConsumption)) habits.Add($"Alcohol: {AlcoholConsumption}");
        if (habits.Any())
            context.AppendLine(string.Join(" | ", habits));

        // Preferences
        if (!string.IsNullOrWhiteSpace(PreferredBaseType))
            context.AppendLine($"Preferred base: {PreferredBaseType}");

        return context.ToString();
    }

    /// <summary>
    /// Clear all profile fields
    /// </summary>
    public void Clear()
    {
        FirstName = null;
        Surname = null;
        Email = null;
        Age = null;
        BiologicalSex = null;
        HeightCm = null;
        WeightKg = null;
        CurrentMedications = null;
        KnownAllergies = null;
        HealthConditions = null;
        DietType = null;
        ActivityLevel = null;
        SmokingStatus = null;
        AlcoholConsumption = null;
        AverageSleepHours = null;
        CaffeineIntake = null;
        PreferredBaseType = null;
    }

    #endregion
}
