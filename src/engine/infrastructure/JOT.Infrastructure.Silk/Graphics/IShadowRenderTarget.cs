namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Represents a depth-only render target for shadow map rendering.
/// </summary>
public interface IShadowRenderTarget : IDisposable
{
    /// <summary>
    /// Gets the depth texture size in pixels (square).
    /// </summary>
    int Size { get; }

    /// <summary>
    /// Starts the depth pass: binds the shadow framebuffer, switches
    /// the viewport to the shadow map size, and clears the depth
    /// buffer. Triangle draws issued until <see cref="End"/> render
    /// their depth only.
    /// </summary>
    void Begin();

    /// <summary>
    /// Ends the depth pass: unbinds the shadow framebuffer and
    /// restores the default framebuffer as the render target.
    /// </summary>
    void End();

    /// <summary>
    /// Binds the depth texture to the specified texture unit for
    /// shadow sampling.
    /// </summary>
    /// <param name="textureUnit">
    /// Zero-based index of the texture unit.
    /// </param>
    void BindDepth(int textureUnit);
}
