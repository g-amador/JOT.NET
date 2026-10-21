using Silk.NET.OpenGL;

namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Silk.NET OpenGL implementation of a vertex buffer.
/// </summary>
internal sealed class SilkVertexBuffer : IVertexBuffer
{
    private const int ComponentsPerVertex = 6;

    private const int Stride = ComponentsPerVertex * sizeof(float);

    private readonly GL _gl;

    private readonly uint _vertexArrayObject;

    private readonly uint _vertexBufferObject;

    /// <summary>
    /// Initializes a new instance of the <see cref="SilkVertexBuffer"/> class.
    ///
    /// The vertex layout is fixed: attribute 0 is an XYZ position and
    /// attribute 1 is an RGB color, both as three floats per vertex.
    /// </summary>
    /// <param name="gl">OpenGL API instance.</param>
    /// <param name="vertices">
    /// Interleaved vertex data. The length must be a multiple of
    /// <c>ComponentsPerVertex</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the vertex data length is not a multiple of
    /// <c>ComponentsPerVertex</c>.
    /// </exception>
    public SilkVertexBuffer(
        GL gl,
        float[] vertices)
    {
        ArgumentNullException.ThrowIfNull(gl);
        ArgumentNullException.ThrowIfNull(vertices);

        if (vertices.Length % ComponentsPerVertex != 0)
        {
            throw new ArgumentException(
                $"Vertex data length must be a multiple of {ComponentsPerVertex}.",
                nameof(vertices));
        }

        _gl = gl;

        VertexCount = vertices.Length / ComponentsPerVertex;

        _vertexArrayObject = _gl.GenVertexArray();
        _gl.BindVertexArray(_vertexArrayObject);

        _vertexBufferObject = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferObject);

        _gl.BufferData(
            BufferTargetARB.ArrayBuffer,
            (ReadOnlySpan<float>)vertices,
            BufferUsageARB.StaticDraw);

        _gl.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            (uint)Stride,
            IntPtr.Zero);

        _gl.EnableVertexAttribArray(0);

        _gl.VertexAttribPointer(
            1,
            3,
            VertexAttribPointerType.Float,
            false,
            (uint)Stride,
            new IntPtr(3 * sizeof(float)));

        _gl.EnableVertexAttribArray(1);
    }

    /// <inheritdoc/>
    public int VertexCount { get; }

    /// <summary>
    /// Binds the vertex array for subsequent rendering.
    /// </summary>
    internal void Use()
    {
        _gl.BindVertexArray(_vertexArrayObject);
    }
}
