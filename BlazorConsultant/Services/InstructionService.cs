using BlazorConsultant.Models;
using BlazorConsultant.Configuration;
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

    public async Task<List<InstructionSection>> GetSectionsAsync(CancellationToken cancellationToken = default)
    {
        // Return cached if available
        if (_cachedSections != null)
            return _cachedSections;

        var client = _httpClientFactory.CreateClient("PythonAPI");

        _logger.LogInformation("Fetching instruction sections");

        // Add timeout guard for HTTP requests
        using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
            TimeoutPolicy.HttpRequestTimeout,
            cancellationToken);

        try
        {
            var response = await client.GetFromJsonAsync<InstructionResponse>("/api/instructions", timeoutCts.Token)
                .ConfigureAwait(false);

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

                _logger.LogInformation("Fetched {SectionCount} instruction sections", _cachedSections.Count);
                return _cachedSections;
            }

            _logger.LogWarning("Received unsuccessful response when fetching instruction sections");
            return new List<InstructionSection>();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred (not parent cancellation)
            _logger.LogWarning("Instruction sections fetch timed out after {Timeout}s",
                TimeoutPolicy.HttpRequestTimeout.TotalSeconds);
            return new List<InstructionSection>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch instruction sections");
            return new List<InstructionSection>();
        }
    }

    public async Task<List<InstructionSection>> GetDefaultSectionsAsync(CancellationToken cancellationToken = default)
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
        await GetSectionsAsync(cancellationToken).ConfigureAwait(false);

        return _defaultSections ?? new List<InstructionSection>();
    }

    public async Task<bool> UpdateSectionsAsync(List<InstructionSection> sections, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        _logger.LogInformation("Updating {SectionCount} instruction sections", sections.Count);

        // Add timeout guard for HTTP requests
        using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
            TimeoutPolicy.HttpRequestTimeout,
            cancellationToken);

        try
        {
            var sectionsDict = sections.ToDictionary(s => s.Name, s => s.Content);

            var response = await client.PostAsJsonAsync("/api/instructions", new
            {
                sections = sectionsDict
            }, timeoutCts.Token).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                // Update cache
                _cachedSections = sections;
                _logger.LogInformation("Successfully updated instruction sections");
                return true;
            }

            _logger.LogWarning("Failed to update instruction sections, status code: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred (not parent cancellation)
            _logger.LogWarning("Instruction sections update timed out after {Timeout}s",
                TimeoutPolicy.HttpRequestTimeout.TotalSeconds);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update instruction sections");
            return false;
        }
    }

    public async Task<bool> UpdateRawInstructionsAsync(string rawInstructions, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        _logger.LogInformation("Updating raw instruction text ({Length} chars)", rawInstructions.Length);

        // Add timeout guard for HTTP requests
        using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
            TimeoutPolicy.HttpRequestTimeout,
            cancellationToken);

        try
        {
            var response = await client.PostAsJsonAsync("/api/instructions", new
            {
                raw_text = rawInstructions
            }, timeoutCts.Token).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                // Clear cached sections since we're using raw mode
                _cachedSections = null;
                _logger.LogInformation("Successfully updated raw instruction text");
                return true;
            }

            _logger.LogWarning("Failed to update raw instruction text, status code: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred (not parent cancellation)
            _logger.LogWarning("Raw instruction text update timed out after {Timeout}s",
                TimeoutPolicy.HttpRequestTimeout.TotalSeconds);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update raw instruction text");
            return false;
        }
    }

    public async Task<string> GetFullTextAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        _logger.LogInformation("Fetching full instruction text");

        // Add timeout guard for HTTP requests
        using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
            TimeoutPolicy.HttpRequestTimeout,
            cancellationToken);

        try
        {
            var response = await client.GetFromJsonAsync<InstructionResponse>("/api/instructions", timeoutCts.Token)
                .ConfigureAwait(false);

            var fullText = response?.FullText ?? string.Empty;
            _logger.LogInformation("Fetched full instruction text ({Length} chars)", fullText.Length);
            return fullText;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred (not parent cancellation)
            _logger.LogWarning("Full instruction text fetch timed out after {Timeout}s",
                TimeoutPolicy.HttpRequestTimeout.TotalSeconds);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch full instruction text");
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
