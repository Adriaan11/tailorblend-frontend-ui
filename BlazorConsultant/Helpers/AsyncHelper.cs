namespace BlazorConsultant.Helpers;

/// <summary>
/// Provides reusable async utility methods for common patterns like throttling,
/// debouncing, and timeout management.
/// </summary>
public static class AsyncHelper
{
    /// <summary>
    /// Wraps an async operation with a timeout.
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Async operation to execute</param>
    /// <param name="timeout">Timeout duration</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>Result of the operation</returns>
    /// <exception cref="OperationCanceledException">Thrown if timeout expires or token is cancelled</exception>
    /// <example>
    /// <code>
    /// var result = await AsyncHelper.WithTimeoutAsync(
    ///     () => httpClient.GetAsync(url),
    ///     TimeSpan.FromSeconds(30),
    ///     cancellationToken
    /// );
    /// </code>
    /// </example>
    public static async Task<T> WithTimeoutAsync<T>(
        Func<Task<T>> operation,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        try
        {
            return await operation().ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred (not parent cancellation)
            throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
        }
    }

    /// <summary>
    /// Wraps an async operation with a timeout (no return value).
    /// </summary>
    /// <param name="operation">Async operation to execute</param>
    /// <param name="timeout">Timeout duration</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <exception cref="OperationCanceledException">Thrown if timeout expires or token is cancelled</exception>
    public static async Task WithTimeoutAsync(
        Func<Task> operation,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        try
        {
            await operation().ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred (not parent cancellation)
            throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
        }
    }
}

/// <summary>
/// Provides throttling for async operations - ensures operations don't execute
/// more frequently than a specified interval.
/// </summary>
/// <remarks>
/// Thread-safe throttling mechanism. If called within the throttle interval,
/// the operation is skipped. Useful for preventing excessive JS interop calls,
/// rapid-fire API requests, or frequent UI updates.
/// </remarks>
/// <example>
/// <code>
/// private readonly AsyncThrottle _scrollThrottle = new(TimeSpan.FromMilliseconds(150));
///
/// private async Task ScrollToBottomAsync()
/// {
///     await _scrollThrottle.ExecuteAsync(async () =>
///     {
///         await JS.InvokeVoidAsync("scrollToBottom", container);
///     });
/// }
/// </code>
/// </example>
public sealed class AsyncThrottle
{
    private readonly TimeSpan _interval;
    private readonly object _lock = new();
    private DateTime _lastExecutionTime = DateTime.MinValue;

    /// <summary>
    /// Creates a new throttle with the specified minimum interval between executions.
    /// </summary>
    /// <param name="interval">Minimum time between executions</param>
    public AsyncThrottle(TimeSpan interval)
    {
        if (interval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be positive");

        _interval = interval;
    }

    /// <summary>
    /// Executes the operation if sufficient time has passed since the last execution.
    /// Returns true if executed, false if throttled.
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>True if operation was executed, false if throttled</returns>
    public async Task<bool> ExecuteAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        bool shouldExecute;

        lock (_lock)
        {
            var elapsed = DateTime.Now - _lastExecutionTime;
            shouldExecute = elapsed >= _interval;

            if (shouldExecute)
            {
                _lastExecutionTime = DateTime.Now;
            }
        }

        if (shouldExecute)
        {
            await operation().ConfigureAwait(false);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Executes the operation if sufficient time has passed since the last execution.
    /// Returns the result if executed, or default(T) if throttled.
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>Operation result if executed, default(T) if throttled</returns>
    public async Task<(bool executed, T? result)> ExecuteAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        bool shouldExecute;

        lock (_lock)
        {
            var elapsed = DateTime.Now - _lastExecutionTime;
            shouldExecute = elapsed >= _interval;

            if (shouldExecute)
            {
                _lastExecutionTime = DateTime.Now;
            }
        }

        if (shouldExecute)
        {
            var result = await operation().ConfigureAwait(false);
            return (true, result);
        }

        return (false, default);
    }

    /// <summary>
    /// Resets the throttle, allowing the next operation to execute immediately.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _lastExecutionTime = DateTime.MinValue;
        }
    }
}
