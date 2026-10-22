namespace JOT.Infrastructure.Assimp.Importing;

/// <summary>
/// Raw RGBA image data.
/// </summary>
public sealed class RawImage
{
    /// <summary>
    /// Gets the path the image was loaded from.
    /// </summary>
    public string SourcePath { get; init; }
        = string.Empty;

    /// <summary>
    /// Gets the image width in pixels.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Gets the image height in pixels.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// Gets the RGBA pixel data, four bytes per pixel, rows ordered
    /// top to bottom.
    /// </summary>
    public required byte[] Pixels { get; init; }
}
