namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Represents a graphics context.
/// </summary>
public interface IGraphicsContext
{
    /// <summary>
    /// Clears the render target using the specified color.
    /// </summary>
    /// <param name="red">Red component (0-1).</param>
    /// <param name="green">Green component (0-1).</param>
    /// <param name="blue">Blue component (0-1).</param>
    /// <param name="alpha">Alpha component (0-1).</param>
    void Clear(
        float red,
        float green,
        float blue,
        float alpha);
}