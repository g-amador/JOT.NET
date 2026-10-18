using Silk.NET.OpenGL;

namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Silk.NET OpenGL graphics context.
/// </summary>
public sealed class SilkGraphicsContext : IGraphicsContext
{
    private readonly GL _gl;

    public SilkGraphicsContext(GL gl)
    {
        _gl = gl;
    }

    /// <inheritdoc/>
    public void Clear(
        float red,
        float green,
        float blue,
        float alpha)
    {
        _gl.ClearColor(
            red,
            green,
            blue,
            alpha);

        _gl.Clear(
            ClearBufferMask.ColorBufferBit);
    }
}