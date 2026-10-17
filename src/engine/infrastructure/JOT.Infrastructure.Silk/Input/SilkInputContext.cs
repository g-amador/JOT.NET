using SilkNetInputContext = Silk.NET.Input.IInputContext;

namespace JOT.Infrastructure.Silk.Input;

/// <summary>
/// Adapts Silk.NET input devices to the JOT input interfaces.
/// </summary>
public sealed class SilkInputContext : IInputContext
{
    /// <summary>
    /// Gets the primary keyboard device.
    /// </summary>
    public IKeyboard Keyboard { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SilkInputContext"/> class.
    /// </summary>
    /// <param name="inputContext">The underlying Silk.NET input context.</param>
    public SilkInputContext(SilkNetInputContext inputContext)
    {
        ArgumentNullException.ThrowIfNull(inputContext);

        var keyboard = inputContext.Keyboards.FirstOrDefault()
            ?? throw new InvalidOperationException(
                "No keyboard device is available.");

        Keyboard = new SilkKeyboard(keyboard);
    }
}
