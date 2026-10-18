using JOT.Infrastructure.Silk.Graphics;
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
    /// Gets the graphics context associated with the window.
    /// </summary>
    IGraphicsContext Graphics { get; }

    /// <summary>
    /// Starts the window loop and invokes the provided callback once per update.
    /// </summary>
    /// <param name="update">
    /// Callback executed every frame. The first parameter is the current window
    /// instance and the second parameter is the elapsed time, in seconds,
    /// since the previous update.
    /// </param>
    void Run(Action<IWindow, double> update);

    /// <summary>
    /// Requests the window to close.
    /// </summary>
    void Close();
}