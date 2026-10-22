namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Represents an uploaded cube map texture.
/// </summary>
public interface ITextureCubeMap : IDisposable
{
    /// <summary>
    /// Binds the cube map to the specified texture unit for
    /// subsequent rendering.
    /// </summary>
    /// <param name="textureUnit">
    /// Zero-based index of the texture unit.
    /// </param>
    void Bind(int textureUnit);
}
