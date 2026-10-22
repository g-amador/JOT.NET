namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Represents an uploaded 2D texture.
/// </summary>
public interface ITexture2D : IDisposable
{
    /// <summary>
    /// Gets the texture width in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the texture height in pixels.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Binds the texture to the specified texture unit for
    /// subsequent rendering.
    /// </summary>
    /// <param name="textureUnit">
    /// Zero-based index of the texture unit.
    /// </param>
    void Bind(int textureUnit);
}
