using System.Numerics;

namespace JOT.Infrastructure.Assimp.Importing;

/// <summary>
/// Imported material description.
///
/// <para><see cref="DiffuseEmbeddedTexture"/> holds textures stored
/// inside the model file itself (glTF binary); the importer decodes
/// them and hands the pixels over directly, so binary models render
/// with their own textures without extra files.</para>
/// </summary>
public sealed class ImportedMaterial
{
    /// <summary>
    /// Gets the material name.
    /// </summary>
    public string Name { get; init; }
        = string.Empty;

    /// <summary>
    /// Gets the diffuse base color.
    /// </summary>
    public Vector3 DiffuseColor { get; init; }
        = Vector3.One;

    /// <summary>
    /// Gets the diffuse texture path as referenced by the model file,
    /// or null when none is defined. Path is resolved by the consumer.
    /// </summary>
    public string? DiffuseTexturePath { get; init; }

    /// <summary>
    /// Gets the diffuse texture pixels stored inside the model file
    /// (glTF binary), or null when the texture comes from a separate
    /// file or is absent.
    /// </summary>
    public RawImage? DiffuseEmbeddedTexture { get; init; }
}
