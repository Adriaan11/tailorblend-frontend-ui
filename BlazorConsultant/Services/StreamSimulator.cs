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
    /// <param name="intervalMs">Milliseconds between reveals (default: 20ms = 1 char every 20ms)</param>
    public void StartSimulation(string fullText, double intervalMs = 20)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StreamSimulator));

        _fullText = fullText;
        _currentIndex = 0;
        CurrentText = "";
        _isPaused = false;

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
                OnTokenRevealed?.Invoke();
                Console.WriteLine($"✅ [StreamSimulator] Simulation complete");
            }
            return;
        }

        // Ultra-simple: Reveal exactly 1 character per tick
        CurrentText += _fullText[_currentIndex];
        _currentIndex++;

        // Notify UI immediately on every character (no batching)
        OnTokenRevealed?.Invoke();
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
