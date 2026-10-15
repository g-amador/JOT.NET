using Silk.NET.Windowing;

namespace JOT.Infrastructure.Silk.Windowing;

public sealed class SilkWindow : JOT.Infrastructure.Silk.Abstractions.IWindow
{
    private readonly IWindow _window;

    public string Title { get; }

    public int Width { get; }

    public int Height { get; }

    public bool IsRunning { get; private set; }

    public SilkWindow(
        string title,
        int width,
        int height)
    {
        Title = title;
        Width = width;
        Height = height;

        var options = WindowOptions.Default;

        options.Title = title;
        options.Size = new(width, height);

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Closing += OnClosing;
    }

    public void Run()
    {
        IsRunning = true;
        _window.Run();
    }

    public void Close()
    {
        _window.Close();
    }

    private void OnLoad()
    {
    }

    private void OnUpdate(double deltaTime)
    {
    }

    private void OnClosing()
    {
        IsRunning = false;
    }
}