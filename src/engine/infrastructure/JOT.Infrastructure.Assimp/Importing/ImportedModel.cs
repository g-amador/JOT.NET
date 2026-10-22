namespace JOT.Infrastructure.Assimp.Importing;

/// <summary>
/// Imported model composed of meshes and materials.
/// </summary>
public sealed class ImportedModel
{
    /// <summary>
    /// Gets the model name.
    /// </summary>
    public string Name { get; init; }
        = string.Empty;

    /// <summary>
    /// Gets the meshes of the model.
    /// </summary>
    public required IReadOnlyList<ImportedMesh> Meshes { get; init; }

    /// <summary>
    /// Gets the materials of the model.
    /// </summary>
    public required IReadOnlyList<ImportedMaterial> Materials { get; init; }
}
