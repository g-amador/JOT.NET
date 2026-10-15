namespace JOT.Infrastructure.Silk.Abstractions;

public interface IWindow
{
    string Title { get; }

    int Width { get; }

    int Height { get; }

    bool IsRunning { get; }

    void Run();

    void Close();
}