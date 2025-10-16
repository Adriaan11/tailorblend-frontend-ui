using BlazorConsultant.Models;

namespace BlazorConsultant.Services;

/// <summary>
/// Multi-agent formulation service interface for Python API communication.
/// </summary>
public interface IMultiAgentService
{
    /// <summary>
    /// Stream multi-agent blend formulation from Python API using SSE.
    /// </summary>
    /// <param name="request">Patient profile and health goals</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of agent steps (JSON objects)</returns>
    IAsyncEnumerable<AgentStepResponse> StreamFormulationAsync(
        MultiAgentRequest request,
        CancellationToken cancellationToken = default);
}
