using JOT.Infrastructure.Silk.Input;
using JOT.Infrastructure.Silk.Timing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SilkWindowing = Silk.NET.Windowing;

namespace JOT.Infrastructure.Silk.Windowing;

/// <summary>
/// Silk.NET implementation of a JOT window.
/// </summary>
public sealed class SilkWindow : IWindow
{
    private readonly SilkWindowing.IWindow _window;

    private readonly SilkTimer _timer = new();

    private SilkInputContext? _input;

    private Action<IWindow>? _update;

    /// <summary>
    /// Gets the window title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the window width in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the window height in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets a value indicating whether the window is currently running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Gets the timer associated with the window.
    /// </summary>
    public Timing.ITimer Timer => _timer;

    /// <summary>
    /// Gets the input context associated with the window.
    /// </summary>
    public Input.IInputContext Input =>
        _input ??
        throw new InvalidOperationException(
            "Input has not been initialized.");

    /// <summary>
    /// Initializes a new instance of the <see cref="SilkWindow"/> class.
    /// </summary>
    /// <param name="title">Window title.</param>
    /// <param name="width">Window width in pixels.</param>
    /// <param name="height">Window height in pixels.</param>
    public SilkWindow(string title, int width, int height)
    {
        Title = title;
        Width = width;
        Height = height;

        var options = SilkWindowing.WindowOptions.Default with
        {
            Title = title,
            Size = new Vector2D<int>(width, height)
        };

        _window = SilkWindowing.Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Closing += OnClosing;
    }

    /// <inheritdoc />
    public void Run(Action<IWindow> update)
    {
        ArgumentNullException.ThrowIfNull(update);

        _update = update;
        IsRunning = true;
        _window.Run();
    }

    /// <inheritdoc />
    public void Close() => _window.Close();

    /// <summary>
    /// Initializes input devices and other runtime services.
    /// </summary>
    private void OnLoad()
    {
        var inputContext = _window.CreateInput();
        _input = new SilkInputContext(inputContext);
    }

    /// <summary>
    /// Updates timing information and checks for the Escape key.
    /// </summary>
    /// <param name="deltaTime">
    /// Time elapsed since the previous frame, in seconds.
    /// </param>
    private void OnUpdate(double deltaTime)
    {
        _timer.DeltaTime = deltaTime;
        _timer.TotalTime += deltaTime;
        _update?.Invoke(this);
    }

    /// <summary>
    /// Handles window shutdown.
    /// </summary>
    private void OnClosing()
    {
        IsRunning = false;
    }
}
