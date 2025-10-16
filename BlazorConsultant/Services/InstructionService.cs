using BlazorConsultant.Models;
using System.Text.Json;

namespace BlazorConsultant.Services;

/// <summary>
/// Instruction service implementation - manages agent instructions via Python API.
/// </summary>
public class InstructionService : IInstructionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<InstructionService> _logger;

    // Cache for sections
    private List<InstructionSection>? _cachedSections;
    private List<InstructionSection>? _defaultSections;

    public InstructionService(
        IHttpClientFactory httpClientFactory,
        ILogger<InstructionService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<InstructionSection>> GetSectionsAsync()
    {
        // Return cached if available
        if (_cachedSections != null)
            return _cachedSections;

        var client = _httpClientFactory.CreateClient("PythonAPI");

        _logger.LogInformation("[INSTRUCTIONS] Fetching instruction sections");

        try
        {
            var response = await client.GetFromJsonAsync<InstructionResponse>("/api/instructions");

            if (response?.Success == true && response.Sections != null)
            {
                _cachedSections = response.Sections
                    .Select(kvp => new InstructionSection
                    {
                        Name = kvp.Key,
                        Content = kvp.Value,
                        LineCount = Math.Max(5, kvp.Value.Split('\n').Length + 2)
                    })
                    .ToList();

                // Cache as default if not already cached
                _defaultSections ??= _cachedSections
                    .Select(s => new InstructionSection
                    {
                        Name = s.Name,
                        Content = s.Content,
                        LineCount = s.LineCount
                    })
                    .ToList();

                return _cachedSections;
            }

            return new List<InstructionSection>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[INSTRUCTIONS] Failed to fetch sections");
            return new List<InstructionSection>();
        }
    }

    public async Task<List<InstructionSection>> GetDefaultSectionsAsync()
    {
        // If defaults are cached, return them
        if (_defaultSections != null)
            return _defaultSections.Select(s => new InstructionSection
            {
                Name = s.Name,
                Content = s.Content,
                LineCount = s.LineCount
            }).ToList();

        // Otherwise, fetch current (which will cache defaults)
        await GetSectionsAsync();

        return _defaultSections ?? new List<InstructionSection>();
    }

    public async Task<bool> UpdateSectionsAsync(List<InstructionSection> sections)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        _logger.LogInformation("[INSTRUCTIONS] Updating instruction sections");

        try
        {
            var sectionsDict = sections.ToDictionary(s => s.Name, s => s.Content);

            var response = await client.PostAsJsonAsync("/api/instructions", new
            {
                sections = sectionsDict
            });

            if (response.IsSuccessStatusCode)
            {
                // Update cache
                _cachedSections = sections;
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[INSTRUCTIONS] Failed to update sections");
            return false;
        }
    }

    public async Task<bool> UpdateRawInstructionsAsync(string rawInstructions)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        _logger.LogInformation("[INSTRUCTIONS] Updating raw instruction text");

        try
        {
            var response = await client.PostAsJsonAsync("/api/instructions", new
            {
                raw_text = rawInstructions
            });

            if (response.IsSuccessStatusCode)
            {
                // Clear cached sections since we're using raw mode
                _cachedSections = null;
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[INSTRUCTIONS] Failed to update raw instructions");
            return false;
        }
    }

    public async Task<string> GetFullTextAsync()
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        _logger.LogInformation("[INSTRUCTIONS] Fetching full instruction text");

        try
        {
            var response = await client.GetFromJsonAsync<InstructionResponse>("/api/instructions");

            return response?.FullText ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[INSTRUCTIONS] Failed to fetch full text");
            return string.Empty;
        }
    }

    // Helper class for API response
    private class InstructionResponse
    {
        public bool Success { get; set; }
        public Dictionary<string, string>? Sections { get; set; }
        public string? FullText { get; set; }
        public string? Error { get; set; }
    }
}
