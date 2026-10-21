namespace JOT.Core.Rendering.Meshes;

/// <summary>
/// Represents renderable mesh geometry.
/// </summary>
public interface IMesh
{
    /// <summary>
    /// Gets the mesh name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the number of vertices contained in the mesh.
    /// </summary>
    int VertexCount { get; }

    /// <summary>
    /// Gets the number of indices contained in the mesh.
    /// </summary>
    int IndexCount { get; }
}