namespace BlazorConsultant.Helpers;

/// <summary>
/// Provides thread-safe disposal tracking and guards against operations on disposed objects.
/// Implements the async disposal pattern with proper race condition protection.
/// </summary>
/// <example>
/// <code>
/// public class MyComponent : IAsyncDisposable
/// {
///     private readonly DisposalGuard _disposalGuard = new();
///
///     private async Task DoWorkAsync()
///     {
///         _disposalGuard.ThrowIfDisposed();
///         // ... safe to proceed
///     }
///
///     public async ValueTask DisposeAsync()
///     {
///         await _disposalGuard.DisposeAsync();
///     }
/// }
/// </code>
/// </example>
public sealed class DisposalGuard : IAsyncDisposable
{
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Gets whether this object has been disposed.
    /// </summary>
    public bool IsDisposed
    {
        get
        {
            lock (_lock)
            {
                return _disposed;
            }
        }
    }

    /// <summary>
    /// Throws ObjectDisposedException if the object has been disposed.
    /// Use this at the start of methods to guard against operations on disposed objects.
    /// </summary>
    /// <param name="objectName">Optional name of the object for exception message</param>
    /// <exception cref="ObjectDisposedException">Thrown if object is disposed</exception>
    public void ThrowIfDisposed(string? objectName = null)
    {
        lock (_lock)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(objectName ?? "Object");
            }
        }
    }

    /// <summary>
    /// Executes an action if the object is not disposed.
    /// Returns true if executed, false if object was disposed.
    /// </summary>
    /// <param name="action">Action to execute if not disposed</param>
    /// <returns>True if action was executed, false if object was disposed</returns>
    public bool ExecuteIfNotDisposed(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        lock (_lock)
        {
            if (_disposed)
                return false;

            action();
            return true;
        }
    }

    /// <summary>
    /// Executes an async function if the object is not disposed.
    /// Returns the result if executed, or default(T) if object was disposed.
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="func">Async function to execute if not disposed</param>
    /// <returns>Function result or default(T) if disposed</returns>
    public async Task<T?> ExecuteIfNotDisposedAsync<T>(Func<Task<T>> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        Task<T>? taskToExecute = null;

        lock (_lock)
        {
            if (_disposed)
                return default;

            // Capture the task inside the lock to ensure atomicity
            // This prevents disposal between the check and task creation
            taskToExecute = func();
        }

        return await taskToExecute.ConfigureAwait(false);
    }

    /// <summary>
    /// Marks this object as disposed. This is idempotent - calling multiple times is safe.
    /// </summary>
    /// <returns>ValueTask representing the async disposal operation</returns>
    public ValueTask DisposeAsync()
    {
        lock (_lock)
        {
            if (_disposed)
                return ValueTask.CompletedTask;

            _disposed = true;
        }

        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
