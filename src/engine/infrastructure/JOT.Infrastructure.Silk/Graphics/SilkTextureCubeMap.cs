using Silk.NET.OpenGL;

namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Silk.NET OpenGL implementation of an RGBA cube map texture.
///
/// Rows of each uploaded face are inverted before upload because
/// decoded image data is ordered top to bottom while the OpenGL
/// cube map face convention is bottom to top.
/// </summary>
internal sealed class SilkTextureCubeMap : ITextureCubeMap
{
    private static readonly TextureTarget[] FaceTargets =
    [
        TextureTarget.TextureCubeMapPositiveX,
        TextureTarget.TextureCubeMapNegativeX,
        TextureTarget.TextureCubeMapPositiveY,
        TextureTarget.TextureCubeMapNegativeY,
        TextureTarget.TextureCubeMapPositiveZ,
        TextureTarget.TextureCubeMapNegativeZ,
    ];

    private readonly GL _gl;

    private readonly uint _texture;

    /// <summary>
    /// Initializes a new instance of the <see cref="SilkTextureCubeMap"/>
    /// class by uploading six RGBA faces in the OpenGL cube map order:
    /// positive X, negative X, positive Y, negative Y, positive Z,
    /// negative Z.
    /// </summary>
    /// <param name="gl">OpenGL API instance.</param>
    /// <param name="width">Face width in pixels.</param>
    /// <param name="height">Face height in pixels.</param>
    /// <param name="faces">
    /// Face pixel data in cube map order. Every entry holds exactly
    /// <c>width * height * 4</c> bytes of RGBA data.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the width or height is not positive.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the face count is not six or a face holds the wrong
    /// amount of pixel data.
    /// </exception>
    public SilkTextureCubeMap(
        GL gl,
        int width,
        int height,
        byte[][] faces)
    {
        ArgumentNullException.ThrowIfNull(gl);
        ArgumentNullException.ThrowIfNull(faces);

        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                width,
                "Cube map face width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                height,
                "Cube map face height must be positive.");
        }

        if (faces.Length != FaceTargets.Length)
        {
            throw new ArgumentException(
                $"Cube maps require exactly {FaceTargets.Length} faces.",
                nameof(faces));
        }

        for (var i = 0; i < faces.Length; ++i)
        {
            if (faces[i] is null)
            {
                throw new ArgumentException(
                    $"Cube map face {i} is null.",
                    nameof(faces));
            }

            if (faces[i].Length != width * height * 4)
            {
                throw new ArgumentException(
                    $"Cube map face {i} must hold {width * height * 4} bytes for a {width}x{height} RGBA face.",
                    nameof(faces));
            }
        }

        _gl = gl;

        _texture = _gl.GenTexture();

        _gl.BindTexture(TextureTarget.TextureCubeMap, _texture);

        for (var i = 0; i < FaceTargets.Length; ++i)
        {
            var pixels = FlipRows(width, height, faces[i]);

            _gl.TexImage2D(
                FaceTargets[i],
                0,
                InternalFormat.Rgba,
                (uint)width,
                (uint)height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                pixels);
        }

        _gl.TexParameter(
            TextureTarget.TextureCubeMap,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);

        _gl.TexParameter(
            TextureTarget.TextureCubeMap,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);

        _gl.TexParameter(
            TextureTarget.TextureCubeMap,
            TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);

        _gl.TexParameter(
            TextureTarget.TextureCubeMap,
            TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);

        _gl.TexParameter(
            TextureTarget.TextureCubeMap,
            TextureParameterName.TextureWrapR,
            (int)TextureWrapMode.ClampToEdge);
    }

    /// <inheritdoc/>
    public void Bind(int textureUnit)
    {
        _gl.ActiveTexture(TextureUnit.Texture0 + textureUnit);

        _gl.BindTexture(TextureTarget.TextureCubeMap, _texture);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _gl.DeleteTexture(_texture);
    }

    private static byte[] FlipRows(
        int width,
        int height,
        byte[] source)
    {
        var rowBytes = width * 4;

        var flipped = new byte[source.Length];

        for (var row = 0; row < height; ++row)
        {
            System.Buffer.BlockCopy(
                source,
                row * rowBytes,
                flipped,
                (height - 1 - row) * rowBytes,
                rowBytes);
        }

        return flipped;
    }
}
