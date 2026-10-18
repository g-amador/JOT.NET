using JOT.Infrastructure.Silk.Input;
using JOT.Infrastructure.Silk.Windowing;

var window = new SilkWindow("JOT.NET", 1280, 720);

window.Run((currentWindow, deltaTime) =>
{
    currentWindow.Graphics.Clear(
        0.1f,
        0.15f,
        0.35f,
        1.0f);

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Escape))
    {
        currentWindow.Close();
    }
});