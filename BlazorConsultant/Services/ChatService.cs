using BlazorConsultant.Models;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace BlazorConsultant.Services;

/// <summary>
/// Chat service implementation - communicates with Python FastAPI backend.
/// Provides HttpClient-based SSE streaming for POST scenarios (attachments, practitioner mode).
/// For text-only GET requests, ChatStateService uses SseStreamManager instead.
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

    /// <summary>
    /// Stream chat response using HttpClient (for POST scenarios only).
    /// NOTE: This is a fallback for attachments and practitioner mode.
    /// Regular text-only streaming uses EventSource via SseStreamManager.
    /// </summary>
    public async IAsyncEnumerable<string> StreamChatAsync(
        string message,
        string? customInstructions = null,
        string? model = null,
        List<FileAttachment>? attachments = null,
        bool practitionerMode = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        // This method only handles POST requests (attachments or practitioner mode)
        var requestBody = new
        {
            message = message,
            session_id = _sessionService.SessionId,
            custom_instructions = customInstructions,
            model = model ?? "gpt-4.1-mini-2025-04-14",
            attachments = attachments ?? new List<FileAttachment>(),
            practitioner_mode = practitionerMode
        };

        _logger.LogInformation($"[CHAT] Sending POST request (practitioner: {practitionerMode}, attachments: {attachments?.Count ?? 0})");

        HttpResponseMessage? response = null;
        Stream? stream = null;
        StreamReader? reader = null;

        try
        {
            // Send POST request
            response = await client.PostAsJsonAsync("/api/chat/stream", requestBody, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            }, cancellationToken);

            response.EnsureSuccessStatusCode();
            stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            reader = new StreamReader(stream);

            // Create timeout for token reading
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            // Read SSE stream line by line
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(line))
                    continue;

                // SSE format: "data: {token}" or ": keepalive"
                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6); // Remove "data: " prefix

                    // Check for completion signal
                    if (data == "[DONE]")
                    {
                        _logger.LogInformation("[CHAT] Stream completed");
                        break;
                    }

                    // Parse JSON token
                    var token = TryDeserializeToken(data);
                    if (!string.IsNullOrEmpty(token))
                    {
                        yield return token;

                        // Reset timeout after receiving token
                        timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
                    }
                }
                else if (line.StartsWith(":"))
                {
                    // SSE keepalive - reset timeout
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
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
