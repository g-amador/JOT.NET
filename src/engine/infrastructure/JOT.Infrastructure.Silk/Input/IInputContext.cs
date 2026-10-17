namespace JOT.Infrastructure.Silk.Input;

/// <summary>
/// Provides access to input devices.
/// </summary>
public interface IInputContext
{
    /// <summary>
    /// Gets the primary keyboard.
    /// </summary>
    IKeyboard Keyboard { get; }
}