using System.Timers;

namespace BlazorConsultant.Services;

/// <summary>
/// Simulates token-by-token streaming by gradually revealing pre-fetched text.
///
/// Provides illusion of real-time streaming without SSE complexity.
/// Allows pause/resume, speed control, and instant completion.
/// </summary>
public class StreamSimulator : IDisposable
{
    private System.Timers.Timer? _timer;
    private string _fullText = "";
    private int _currentIndex = 0;
    private bool _disposed;
    private bool _isPaused;
    // ✅ PERFORMANCE: Event batching counter to reduce SignalR overhead
    private int _eventCounter = 0;

    /// <summary>
    /// Currently revealed text (grows over time)
    /// </summary>
    public string CurrentText { get; private set; } = "";

    /// <summary>
    /// True if all text has been revealed
    /// </summary>
    public bool IsComplete => _currentIndex >= _fullText.Length;

    /// <summary>
    /// Fired whenever new characters are revealed
    /// </summary>
    public event Action? OnTokenRevealed;

    /// <summary>
    /// Start simulating streaming of the full text.
    /// </summary>
    /// <param name="fullText">Complete response to reveal gradually</param>
    /// <param name="intervalMs">Milliseconds between reveals (default: 10ms = 100+ tokens/sec)</param>
    public void StartSimulation(string fullText, double intervalMs = 10)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StreamSimulator));

        _fullText = fullText;
        _currentIndex = 0;
        CurrentText = "";
        _isPaused = false;
        _eventCounter = 0; // ✅ PERFORMANCE: Reset batch counter for new simulation

        _timer = new System.Timers.Timer(intervalMs);
        _timer.Elapsed += RevealNextChunk;
        _timer.AutoReset = true;
        _timer.Start();

        Console.WriteLine($"▶️ [StreamSimulator] Started simulation: {fullText.Length} chars at {intervalMs}ms intervals");
    }

    private void RevealNextChunk(object? sender, ElapsedEventArgs e)
    {
        if (_disposed || _isPaused || _currentIndex >= _fullText.Length)
        {
            if (_currentIndex >= _fullText.Length)
            {
                _timer?.Stop();
                // ✅ PERFORMANCE: Always notify on completion
                OnTokenRevealed?.Invoke();
                Console.WriteLine($"✅ [StreamSimulator] Simulation complete");
            }
            return;
        }

        // Reveal 2-5 characters per tick for faster streaming (100+ tokens/sec)
        // Still vary based on character type for natural appearance
        int chunkSize = DetermineChunkSize();

        int remaining = _fullText.Length - _currentIndex;
        int toReveal = Math.Min(chunkSize, remaining);

        CurrentText += _fullText.Substring(_currentIndex, toReveal);
        _currentIndex += toReveal;
        _eventCounter++;

        // ✅ PERFORMANCE: Batch events - only notify every 3 chunks (100/sec → 33/sec)
        // This reduces SignalR circuit messages while maintaining smooth appearance
        if (_eventCounter % 3 == 0 || _currentIndex >= _fullText.Length)
        {
            OnTokenRevealed?.Invoke();
        }
    }

    private int DetermineChunkSize()
    {
        if (_currentIndex >= _fullText.Length)
            return 1;

        char currentChar = _fullText[_currentIndex];

        // Slight pause on sentence boundaries (2 chars = brief pause)
        if (currentChar == '.' || currentChar == '!' || currentChar == '?')
            return 2;

        // Fast on whitespace (3-6 chars)
        if (char.IsWhiteSpace(currentChar))
            return Random.Shared.Next(3, 7);

        // Normal speed for letters/numbers (2-5 chars for 100+ tokens/sec)
        return Random.Shared.Next(2, 6);
    }

    /// <summary>
    /// Pause the simulation (can resume later)
    /// </summary>
    public void Pause()
    {
        _isPaused = true;
        Console.WriteLine($"⏸️ [StreamSimulator] Paused at {_currentIndex}/{_fullText.Length}");
    }

    /// <summary>
    /// Resume after pausing
    /// </summary>
    public void Resume()
    {
        _isPaused = false;
        Console.WriteLine($"▶️ [StreamSimulator] Resumed");
    }

    /// <summary>
    /// Stop simulation and instantly reveal all remaining text.
    /// Used when user clicks "Skip" or cancels.
    /// </summary>
    public void CompleteInstantly()
    {
        if (_disposed)
            return;

        Console.WriteLine($"⏭️ [StreamSimulator] Completing instantly ({_fullText.Length - _currentIndex} chars remaining)");

        _timer?.Stop();
        CurrentText = _fullText;
        _currentIndex = _fullText.Length;
        OnTokenRevealed?.Invoke();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _timer?.Stop();
        _timer?.Dispose();

        Console.WriteLine($"🗑️ [StreamSimulator] Disposed");
    }
}
