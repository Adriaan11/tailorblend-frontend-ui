using BlazorConsultant.Models;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace BlazorConsultant.Services;

/// <summary>
/// Multi-agent formulation service implementation.
/// Communicates with Python FastAPI backend for multi-agent blend creation.
/// Handles SSE streaming for real-time agent progress updates.
/// </summary>
public class MultiAgentService : IMultiAgentService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MultiAgentService> _logger;

    public MultiAgentService(
        IHttpClientFactory httpClientFactory,
        ILogger<MultiAgentService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async IAsyncEnumerable<AgentStepResponse> StreamFormulationAsync(
        MultiAgentRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("PythonAPI");

        _logger.LogInformation($"[MULTI-AGENT] Starting formulation for session: {request.SessionId}");

        HttpResponseMessage? response = null;
        Stream? stream = null;
        StreamReader? reader = null;

        try
        {
            // POST request with JSON body
            response = await client.PostAsJsonAsync("/api/multi-agent/stream", request, cancellationToken);

            _logger.LogInformation($"[MULTI-AGENT] Response status: {response.StatusCode}");
            response.EnsureSuccessStatusCode();

            stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            reader = new StreamReader(stream);

            // Read SSE stream
            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // SSE format: "data: {json}"
                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6); // Remove "data: " prefix

                    // Check for completion signal
                    if (data == "[DONE]")
                    {
                        _logger.LogInformation("[MULTI-AGENT] Stream completed");
                        break;
                    }

                    // Parse JSON agent step - wrap in try without yield
                    AgentStepResponse? step = null;
                    try
                    {
                        step = JsonSerializer.Deserialize<AgentStepResponse>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, $"[MULTI-AGENT] Failed to parse agent step: {data}");
                    }

                    if (step != null)
                    {
                        _logger.LogInformation($"[MULTI-AGENT] {step.AgentName}: {step.StepType}");
                        yield return step;
                    }
                }
            }
        }
        finally
        {
            reader?.Dispose();
            stream?.Dispose();
            response?.Dispose();
        }
    }
}
