namespace JOT.Core.Rendering.Textures;

/// <summary>
/// Represents a texture resource.
/// </summary>
public interface ITexture
{
    /// <summary>
    /// Gets the texture name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the texture width.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the texture height.
    /// </summary>
    int Height { get; }
}