using System.Numerics;

namespace JOT.Infrastructure.Assimp.Importing;

/// <summary>
/// Imported material description.
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
}
