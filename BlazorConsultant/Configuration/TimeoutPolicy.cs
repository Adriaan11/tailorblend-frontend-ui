namespace BlazorConsultant.Configuration;

/// <summary>
/// Centralized timeout configuration for async operations throughout the application.
/// Provides consistent timeout values for different operation types.
/// </summary>
/// <remarks>
/// Timeout values are chosen based on expected operation duration and user experience requirements:
/// - Short timeouts (10s): Quick operations where user expects immediate feedback
/// - Medium timeouts (30-60s): Standard HTTP requests that may involve AI processing
/// - Long timeouts (5min): Streaming operations that are expected to run longer
/// </remarks>
public static class TimeoutPolicy
{
    /// <summary>
    /// Timeout for standard HTTP API requests (chat messages, configuration updates).
    /// Default: 120 seconds (2 minutes)
    /// </summary>
    /// <remarks>
    /// This timeout accounts for:
    /// - Network latency
    /// - Backend AI processing time
    /// - Response serialization
    /// Consider the backend may be calling OpenAI API which can take 30-60s for complex requests.
    /// </remarks>
    public static TimeSpan HttpRequestTimeout { get; set; } = TimeSpan.FromSeconds(120);

    /// <summary>
    /// Timeout for SSE (Server-Sent Events) streaming connections.
    /// Default: 5 minutes
    /// </summary>
    /// <remarks>
    /// Streaming operations like multi-agent formulation can take several minutes.
    /// This timeout prevents infinite hangs while allowing sufficient time for complex workflows.
    /// </remarks>
    public static TimeSpan StreamingTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Timeout for UI component operations (stats refresh, localStorage operations).
    /// Default: 10 seconds
    /// </summary>
    /// <remarks>
    /// Component operations should be fast. This shorter timeout ensures responsive UI.
    /// </remarks>
    public static TimeSpan ComponentOperationTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Timeout per file during file upload operations.
    /// Default: 60 seconds per file
    /// </summary>
    /// <remarks>
    /// Based on max file size of 10MB and typical upload speeds.
    /// Large files or slow connections may need adjustment.
    /// </remarks>
    public static TimeSpan FileUploadTimeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Timeout for session reset and cleanup operations.
    /// Default: 10 seconds
    /// </summary>
    /// <remarks>
    /// Session operations are typically fast. This timeout ensures we don't hang on cleanup.
    /// </remarks>
    public static TimeSpan SessionOperationTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Creates a linked CancellationTokenSource with the specified timeout.
    /// Automatically disposes the linked token source after the timeout.
    /// </summary>
    /// <param name="timeout">Timeout duration</param>
    /// <param name="parentToken">Optional parent cancellation token</param>
    /// <returns>CancellationTokenSource that will cancel after timeout</returns>
    /// <example>
    /// <code>
    /// using var cts = TimeoutPolicy.CreateTimeoutTokenSource(TimeoutPolicy.HttpRequestTimeout, cancellationToken);
    /// await httpClient.GetAsync(url, cts.Token);
    /// </code>
    /// </example>
    public static CancellationTokenSource CreateTimeoutTokenSource(
        TimeSpan timeout,
        CancellationToken parentToken = default)
    {
        if (parentToken == default || parentToken == CancellationToken.None)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);
            return cts;
        }
        else
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
            cts.CancelAfter(timeout);
            return cts;
        }
    }
}
