using Silk.NET.OpenGL;

namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Silk.NET OpenGL implementation of a depth-only shadow render
/// target.
/// </summary>
internal sealed class SilkShadowRenderTarget : IShadowRenderTarget
{
    private readonly GL _gl;

    private readonly uint _framebuffer;

    private readonly uint _depthTexture;

    /// <summary>
    /// Initializes a new instance of the <see cref="SilkShadowRenderTarget"/>
    /// class: a framebuffer with only a depth attachment of the
    /// specified size, usable as the depth pass target and the sampled
    /// shadow map.
    /// </summary>
    /// <param name="gl">OpenGL API instance.</param>
    /// <param name="size">Depth texture size in pixels (square).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the size is not positive.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the framebuffer is incomplete.
    /// </exception>
    public SilkShadowRenderTarget(
        GL gl,
        int size)
    {
        ArgumentNullException.ThrowIfNull(gl);

        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size),
                size,
                "Shadow render target size must be positive.");
        }

        _gl = gl;

        Size = size;

        _depthTexture = _gl.GenTexture();

        _gl.BindTexture(TextureTarget.Texture2D, _depthTexture);

        _gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            InternalFormat.DepthComponent24,
            (uint)size,
            (uint)size,
            0,
            PixelFormat.DepthComponent,
            PixelType.Float,
            (byte[]?)null);

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Nearest);

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);

        _framebuffer = _gl.GenFramebuffer();

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

        _gl.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            TextureTarget.Texture2D,
            _depthTexture,
            0);

        _gl.DrawBuffer(DrawBufferMode.None);

        _gl.ReadBuffer(ReadBufferMode.None);

        if (_gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer)
            != (GLEnum)FramebufferStatus.Complete)
        {
            throw new InvalidOperationException(
                "Shadow framebuffer is incomplete.");
        }

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    /// <inheritdoc/>
    public int Size { get; }

    /// <inheritdoc/>
    public void Begin()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

        _gl.Viewport(0, 0, (uint)Size, (uint)Size);

        _gl.Clear(ClearBufferMask.DepthBufferBit);
    }

    /// <inheritdoc/>
    public void End()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    /// <inheritdoc/>
    public void BindDepth(int textureUnit)
    {
        _gl.ActiveTexture(TextureUnit.Texture0 + textureUnit);

        _gl.BindTexture(TextureTarget.Texture2D, _depthTexture);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _gl.DeleteFramebuffer(_framebuffer);

        _gl.DeleteTexture(_depthTexture);
    }
}
