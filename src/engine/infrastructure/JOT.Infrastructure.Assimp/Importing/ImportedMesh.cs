namespace JOT.Infrastructure.Assimp.Importing;

/// <summary>
/// Imported mesh geometry in plain arrays.
///
/// Positions and normals hold three floats per vertex; UV coordinates
/// hold two floats per vertex. Every array covers every vertex and is
/// zero-filled when the source provides no data.
/// </summary>
public sealed class ImportedMesh
{
    /// <summary>
    /// Gets the mesh name.
    /// </summary>
    public string Name { get; init; }
        = string.Empty;

    /// <summary>
    /// Gets the XYZ positions, three floats per vertex.
    /// </summary>
    public required float[] Positions { get; init; }

    /// <summary>
    /// Gets the XYZ normals, three floats per vertex.
    /// </summary>
    public required float[] Normals { get; init; }

    /// <summary>
    /// Gets the UV coordinates, two floats per vertex.
    /// </summary>
    public required float[] UvCoordinates { get; init; }

    /// <summary>
    /// Gets the triangle indices. The length is a multiple of three.
    /// </summary>
    public required uint[] Indices { get; init; }

    /// <summary>
    /// Gets the material index into <see cref="ImportedModel.Materials"/>.
    /// </summary>
    public int MaterialIndex { get; init; }

    /// <summary>
    /// Gets the number of vertices.
    /// </summary>
    public int VertexCount => Positions.Length / 3;
}
