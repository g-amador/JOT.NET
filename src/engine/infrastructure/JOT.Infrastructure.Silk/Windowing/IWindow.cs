using JOT.Infrastructure.Silk.Input;

namespace JOT.Infrastructure.Silk.Windowing;

/// <summary>
/// Represents an application window.
/// </summary>
public interface IWindow
{
    /// <summary>
    /// Gets the window title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the window width in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the window height in pixels.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets a value indicating whether the window is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the timer associated with the window.
    /// </summary>
    Timing.ITimer Timer { get; }

    /// <summary>
    /// Gets the input context associated with the window.
    /// </summary>
    IInputContext Input { get; }

    /// <summary>
    /// Called once per frame while the window is running.
    /// </summary>
    void Run(Action<IWindow> update);

    /// <summary>
    /// Requests the window to close.
    /// </summary>
    void Close();
}