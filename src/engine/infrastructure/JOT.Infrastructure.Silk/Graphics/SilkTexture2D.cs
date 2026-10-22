using Silk.NET.OpenGL;

namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Silk.NET OpenGL implementation of an RGBA 2D texture.
/// </summary>
internal sealed class SilkTexture2D : ITexture2D
{
    private readonly GL _gl;

    private readonly uint _texture;

    /// <summary>
    /// Initializes a new instance of the <see cref="SilkTexture2D"/> class by
    /// uploading RGBA pixel data to the graphics device.
    /// </summary>
    /// <param name="gl">OpenGL API instance.</param>
    /// <param name="width">Texture width in pixels.</param>
    /// <param name="height">Texture height in pixels.</param>
    /// <param name="pixels">
    /// RGBA pixel data. The length must be exactly
    /// <c>width * height * 4</c> bytes.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the width or height is not positive.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the pixel data length does not match the dimensions.
    /// </exception>
    public SilkTexture2D(
        GL gl,
        int width,
        int height,
        byte[] pixels)
    {
        ArgumentNullException.ThrowIfNull(gl);
        ArgumentNullException.ThrowIfNull(pixels);

        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                width,
                "Texture width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                height,
                "Texture height must be positive.");
        }

        if (pixels.Length != width * height * 4)
        {
            throw new ArgumentException(
                $"Pixel data length must be {width * height * 4} bytes for a {width}x{height} RGBA texture.",
                nameof(pixels));
        }

        _gl = gl;

        Width = width;
        Height = height;

        _texture = _gl.GenTexture();

        _gl.BindTexture(TextureTarget.Texture2D, _texture);

        _gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            InternalFormat.Rgba,
            (uint)width,
            (uint)height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            pixels);

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.Repeat);

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.Repeat);
    }

    /// <inheritdoc/>
    public int Width { get; }

    /// <inheritdoc/>
    public int Height { get; }

    /// <inheritdoc/>
    public void Bind(int textureUnit)
    {
        _gl.ActiveTexture(TextureUnit.Texture0 + textureUnit);

        _gl.BindTexture(TextureTarget.Texture2D, _texture);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _gl.DeleteTexture(_texture);
    }
}
