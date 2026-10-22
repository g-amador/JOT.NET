namespace JOT.Core.Rendering.Sky;

/// <summary>
/// Selects the active sky container.
/// </summary>
public enum SkyMode
{
    /// <summary>
    /// No sky container is rendered.
    /// </summary>
    Off = 0,

    /// <summary>
    /// A six-face sky box is rendered.
    /// </summary>
    Box = 1,

    /// <summary>
    /// A panoramic sky dome is rendered.
    /// </summary>
    Dome = 2,
}
