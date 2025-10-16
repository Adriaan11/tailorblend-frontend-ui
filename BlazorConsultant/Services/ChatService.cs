using BlazorConsultant.Models;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace BlazorConsultant.Services;

/// <summary>
/// Chat service implementation - communicates with Python FastAPI backend.
/// Handles SSE streaming for real-time chat responses.
/// </summary>
public class ChatService : IChatService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISessionService _sessionService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IHttpClientFactory httpClientFactory,
        ISessionService sessionService,
        ILogger<ChatService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _sessionService = sessionService;
        _logger = logger;
    }

    public async IAsyncEnumerable<string> StreamChatAsync(
        string message,
        string? customInstructions = null,
        string? model = null,
        List<FileAttachment>? attachments = null,
        bool practitionerMode = false)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        HttpResponseMessage? response = null;
        Stream? stream = null;
        StreamReader? reader = null;

        // Determine if we have attachments or practitioner mode (both require POST)
        bool hasAttachments = attachments != null && attachments.Count > 0;
        bool requiresPost = hasAttachments || practitionerMode;

        if (requiresPost)
        {
            // POST request with JSON body (for attachments and/or practitioner mode)
            var requestBody = new
            {
                message = message,
                session_id = _sessionService.SessionId,
                custom_instructions = customInstructions,
                model = model ?? "gpt-4.1-mini-2025-04-14",
                attachments = attachments ?? new List<FileAttachment>(),
                practitioner_mode = practitionerMode
            };

            if (practitionerMode)
            {
                _logger.LogInformation($"[CHAT] Sending POST request in practitioner mode with {attachments?.Count ?? 0} attachment(s)");
            }
            else
            {
                _logger.LogInformation($"[CHAT] Sending POST request with {attachments!.Count} attachment(s)");
            }

            // Log attachment details
            foreach (var attachment in attachments)
            {
                _logger.LogInformation($"[CHAT]   - {attachment.FileName} ({attachment.MimeType}, {attachment.FileSize} bytes, base64 length: {attachment.Base64Data?.Length ?? 0})");
            }

            try
            {
                // Setup connection outside iterator
                response = await client.PostAsJsonAsync("/api/chat/stream", requestBody, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                _logger.LogInformation($"[CHAT] POST response status: {response.StatusCode}");
                response.EnsureSuccessStatusCode();
                stream = await response.Content.ReadAsStreamAsync();
                reader = new StreamReader(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[CHAT] POST request failed: {ex.Message}");
                throw;
            }
        }
        else
        {
            // GET request with query parameters (backward compatible, text-only)
            var queryParams = new Dictionary<string, string>
            {
                { "message", message },
                { "session_id", _sessionService.SessionId }
            };

            if (!string.IsNullOrEmpty(customInstructions))
            {
                queryParams["custom_instructions"] = customInstructions;
            }

            if (!string.IsNullOrEmpty(model))
            {
                queryParams["model"] = model;
            }

            var queryString = string.Join("&", queryParams.Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            var url = $"/api/chat/stream?{queryString}";

            _logger.LogInformation($"[CHAT] Sending GET request (text-only)");

            // Setup connection outside iterator
            response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            stream = await response.Content.ReadAsStreamAsync();
            reader = new StreamReader(stream);
        }

        // Now we can use try-finally (which allows yield)
        try
        {
            // Read SSE stream line by line
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(line))
                    continue;

                // SSE format: "data: {token}"
                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6); // Remove "data: " prefix

                    // Check for completion signal
                    if (data == "[DONE]")
                    {
                        _logger.LogInformation("[CHAT] Stream completed");
                        break;
                    }

                    // Parse JSON token - no try-catch here since we're yielding
                    var token = TryDeserializeToken(data);
                    if (!string.IsNullOrEmpty(token))
                    {
                        yield return token;
                    }
                }
            }
        }
        finally
        {
            // Clean up resources
            reader?.Dispose();
            stream?.Dispose();
            response?.Dispose();
        }
    }

    private string? TryDeserializeToken(string data)
    {
        try
        {
            return JsonSerializer.Deserialize<string>(data);
        }
        catch
        {
            // If JSON parsing fails, return raw data
            return data;
        }
    }

    public async Task<SessionStats> GetSessionStatsAsync()
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        var url = $"/api/session/stats?session_id={_sessionService.SessionId}";

        _logger.LogInformation($"[CHAT] Fetching session stats: {url}");

        try
        {
            var stats = await client.GetFromJsonAsync<SessionStats>(url);
            return stats ?? new SessionStats { SessionId = _sessionService.SessionId };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CHAT] Failed to fetch session stats");
            return new SessionStats { SessionId = _sessionService.SessionId };
        }
    }

    public async Task ResetSessionAsync()
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        var url = "/api/session/reset";

        _logger.LogInformation($"[CHAT] Resetting session: {_sessionService.SessionId}");

        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionService.SessionId)
            });

            await client.PostAsync(url, content);

            // Also reset local session
            _sessionService.Reset();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CHAT] Failed to reset session");
        }
    }
}
