using StbImageSharp;

namespace JOT.Infrastructure.Assimp.Importing;

/// <summary>
/// StbImageSharp implementation of an image loader. Decodes PNG, JPG,
/// BMP, TGA, GIF, HDR, and PSD files into RGBA pixel data.
/// </summary>
public sealed class StbImageLoader : IImageLoader
{
    /// <inheritdoc/>
    public RawImage Load(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Image file not found: '{filePath}'.",
                filePath);
        }

        var bytes = File.ReadAllBytes(filePath);

        var result = ImageResult.FromMemory(
            bytes,
            ColorComponents.RedGreenBlueAlpha);

        return new RawImage
        {
            SourcePath = filePath,
            Width = result.Width,
            Height = result.Height,
            Pixels = result.Data,
        };
    }
}
