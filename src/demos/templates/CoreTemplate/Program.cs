using JOT.Infrastructure.Silk.Input;
using JOT.Infrastructure.Silk.Windowing;

var window = new SilkWindow("JOT.NET", 1280, 720);

window.Run((currentWindow, deltaTime) =>
{
    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Escape))
    {
        currentWindow.Close();
    }
});