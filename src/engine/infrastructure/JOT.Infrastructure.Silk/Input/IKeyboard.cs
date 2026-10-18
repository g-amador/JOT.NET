namespace JOT.Infrastructure.Silk.Input;

/// <summary>
/// Represents a keyboard input device.
/// </summary>
public interface IKeyboard
{
    /// <summary>
    /// Determines whether the specified key is currently pressed.
    /// </summary>
    bool IsKeyPressed(KeyCode key);
}