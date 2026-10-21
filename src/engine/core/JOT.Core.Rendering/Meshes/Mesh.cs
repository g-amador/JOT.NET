namespace JOT.Core.Rendering.Meshes;

/// <summary>
/// Represents renderable mesh geometry with interleaved vertices.
///
/// The vertex layout is fixed: attribute 0 is an XYZ position and
/// attribute 1 is an RGB color, both as three floats per vertex.
/// </summary>
public sealed class Mesh : IMesh
{
    /// <summary>
    /// Gets the number of float components per vertex.
    /// </summary>
    public const int ComponentsPerVertex = 6;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mesh"/> class.
    /// </summary>
    /// <param name="name">Mesh name.</param>
    /// <param name="vertices">
    /// Interleaved vertex data. The length must be a multiple of
    /// <c>ComponentsPerVertex</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the vertex data length is not a multiple of
    /// <c>ComponentsPerVertex</c>.
    /// </exception>
    public Mesh(
        string name,
        float[] vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        if (vertices.Length % ComponentsPerVertex != 0)
        {
            throw new ArgumentException(
                $"Vertex data length must be a multiple of {ComponentsPerVertex}.",
                nameof(vertices));
        }

        Name = name;
        Vertices = (float[])vertices.Clone();
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Gets the interleaved vertex data.
    /// </summary>
    public float[] Vertices { get; }

    /// <inheritdoc />
    public int VertexCount => Vertices.Length / ComponentsPerVertex;

    /// <inheritdoc />
    public int IndexCount => 0;
}
