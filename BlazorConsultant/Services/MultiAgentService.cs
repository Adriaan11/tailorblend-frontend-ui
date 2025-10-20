using BlazorConsultant.Models;
using BlazorConsultant.Configuration;
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

        _logger.LogInformation("Starting multi-agent formulation for session {SessionId}", request.SessionId);

        HttpResponseMessage? response = null;
        Stream? stream = null;
        StreamReader? reader = null;

        // Use streaming timeout (longer than regular HTTP requests)
        using var timeoutCts = TimeoutPolicy.CreateTimeoutTokenSource(
            TimeoutPolicy.StreamingTimeout,
            cancellationToken);

        // POST request with JSON body - initial HTTP call
        // Use try-catch only for initial HTTP request (before yield)
        try
        {
            response = await client.PostAsJsonAsync("/api/multi-agent/stream", request, timeoutCts.Token)
                .ConfigureAwait(false);

            _logger.LogInformation("Multi-agent stream response status: {StatusCode}", response.StatusCode);
            response.EnsureSuccessStatusCode();

            stream = await response.Content.ReadAsStreamAsync(timeoutCts.Token)
                .ConfigureAwait(false);
            reader = new StreamReader(stream);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred on initial request (not parent cancellation)
            _logger.LogWarning("Multi-agent formulation connection timed out after {Timeout}s for session {SessionId}",
                TimeoutPolicy.StreamingTimeout.TotalSeconds, request.SessionId);

            // Clean up resources
            reader?.Dispose();
            stream?.Dispose();
            response?.Dispose();

            throw new TimeoutException($"Multi-agent formulation connection timed out after {TimeoutPolicy.StreamingTimeout.TotalSeconds} seconds");
        }
        catch (HttpRequestException ex)
        {
            // HTTP connection error (network issue, DNS failure, etc.)
            _logger.LogError(ex, "HTTP connection failed for multi-agent formulation (session {SessionId}): {Message}",
                request.SessionId, ex.Message);

            // Clean up resources
            reader?.Dispose();
            stream?.Dispose();
            response?.Dispose();
            throw;
        }
        catch (Exception ex)
        {
            // Unexpected error - log for diagnostics
            _logger.LogError(ex, "Unexpected error starting multi-agent formulation for session {SessionId}: {ExceptionType} - {Message}",
                request.SessionId, ex.GetType().Name, ex.Message);

            // Clean up resources
            reader?.Dispose();
            stream?.Dispose();
            response?.Dispose();
            throw;
        }

        // Stream reading loop - no try-catch to allow yield return
        // Clean up in finally block
        try
        {
            // Read SSE stream
            while (!reader.EndOfStream && !timeoutCts.Token.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(timeoutCts.Token)
                    .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // SSE format: "data: {json}"
                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6); // Remove "data: " prefix

                    // Check for completion signal
                    if (data == "[DONE]")
                    {
                        _logger.LogInformation("Multi-agent stream completed for session {SessionId}", request.SessionId);
                        break;
                    }

                    // Parse JSON agent step - log errors but don't throw
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
                        _logger.LogError(ex, "Failed to parse multi-agent step data: {Data}", data);
                    }

                    if (step != null)
                    {
                        _logger.LogInformation("Multi-agent step: {AgentName} - {StepType}", step.AgentName, step.StepType);
                        yield return step;
                    }
                }
            }

            if (timeoutCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Multi-agent stream timed out after {Timeout}s for session {SessionId}",
                    TimeoutPolicy.StreamingTimeout.TotalSeconds, request.SessionId);
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
