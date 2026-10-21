namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Represents a vertex buffer uploaded to the graphics device.
/// </summary>
public interface IVertexBuffer
{
    /// <summary>
    /// Gets the number of vertices contained in the buffer.
    /// </summary>
    int VertexCount { get; }
}
