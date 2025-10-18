using BlazorConsultant.Models;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace BlazorConsultant.Services;

/// <summary>
/// Chat service implementation - communicates with Python FastAPI backend.
/// Uses simple HTTP POST (non-streaming) - frontend simulates streaming for better UX.
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
    /// Send chat message and receive complete response (non-streaming).
    /// Backend accumulates full response, frontend simulates streaming.
    /// </summary>
    public async Task<ChatResponse> SendChatAsync(
        string message,
        string? customInstructions = null,
        string? model = null,
        List<FileAttachment>? attachments = null,
        bool practitionerMode = false,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        var requestBody = new
        {
            message,
            session_id = _sessionService.SessionId,
            custom_instructions = customInstructions,
            model = model ?? "gpt-4.1-mini-2025-04-14",
            attachments = attachments ?? new List<FileAttachment>(),
            practitioner_mode = practitionerMode
        };

        _logger.LogInformation($"[CHAT] Sending non-streaming request (attachments: {attachments?.Count ?? 0})");

        var response = await client.PostAsJsonAsync("/api/chat", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var chatResponse = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken);

        _logger.LogInformation($"[CHAT] Received response: {chatResponse?.Response.Length ?? 0} chars");

        return chatResponse ?? throw new Exception("Empty response from backend");
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
