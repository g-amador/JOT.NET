namespace JOT.Infrastructure.Assimp.Importing;

/// <summary>
/// Loads image files into raw RGBA pixel data.
///
/// This is the asset glue contract for third-party image decoding
/// libraries; it deals in plain data types only.
/// </summary>
public interface IImageLoader
{
    /// <summary>
    /// Loads the image at the specified path into RGBA pixel data.
    /// </summary>
    /// <param name="filePath">Path of the image file.</param>
    /// <returns>
    /// The loaded image, with four bytes per pixel arranged
    /// red, green, blue, alpha.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the image file does not exist.
    /// </exception>
    RawImage Load(string filePath);
}
