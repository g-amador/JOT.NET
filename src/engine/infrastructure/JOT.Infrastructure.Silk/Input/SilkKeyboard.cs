using JotKeyCode = JOT.Infrastructure.Silk.Input.KeyCode;
using SilkKey = Silk.NET.Input.Key;
using SilkKeyboardDevice = Silk.NET.Input.IKeyboard;

namespace JOT.Infrastructure.Silk.Input;

/// <summary>
/// Adapts a Silk.NET keyboard to the JOT keyboard interface.
/// </summary>
internal sealed class SilkKeyboard : IKeyboard
{
    private readonly SilkKeyboardDevice _keyboard;

    /// <summary>
    /// Initializes a new instance of the <see cref="SilkKeyboard"/> class.
    /// </summary>
    /// <param name="keyboard">The underlying Silk.NET keyboard.</param>
    public SilkKeyboard(SilkKeyboardDevice keyboard)
    {
        ArgumentNullException.ThrowIfNull(keyboard);
        _keyboard = keyboard;
    }

    /// <inheritdoc />
    public bool IsKeyPressed(JotKeyCode key)
    {
        if (!Enum.TryParse<SilkKey>(key.ToString(), out var silkKey))
        {
            throw new ArgumentOutOfRangeException(
                nameof(key),
                key,
                "This key is not supported by Silk.NET.");
        }

        return _keyboard.IsKeyPressed(silkKey);
    }
}
