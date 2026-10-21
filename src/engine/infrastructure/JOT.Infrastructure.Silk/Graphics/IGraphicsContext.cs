namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Represents a graphics context.
/// </summary>
public interface IGraphicsContext
{
    /// <summary>
    /// Clears the render target using the specified color.
    /// </summary>
    /// <param name="red">Red component (0-1).</param>
    /// <param name="green">Green component (0-1).</param>
    /// <param name="blue">Blue component (0-1).</param>
    /// <param name="alpha">Alpha component (0-1).</param>
    void Clear(
        float red,
        float green,
        float blue,
        float alpha);

    /// <summary>
    /// Compiles the specified shader sources into a shader program.
    /// </summary>
    /// <param name="vertexShaderSource">
    /// GLSL source of the vertex shader.
    /// </param>
    /// <param name="fragmentShaderSource">
    /// GLSL source of the fragment shader.
    /// </param>
    /// <returns>
    /// The compiled shader program.
    /// </returns>
    IShaderProgram CreateShaderProgram(
        string vertexShaderSource,
        string fragmentShaderSource);

    /// <summary>
    /// Uploads the specified vertex data to the graphics device.
    ///
    /// The vertex layout is fixed: attribute 0 is an XYZ position and
    /// attribute 1 is an RGB color, both as three floats per vertex.
    /// </summary>
    /// <param name="vertices">
    /// Interleaved vertex data. The length must be a multiple of six.
    /// </param>
    /// <returns>
    /// The uploaded vertex buffer.
    /// </returns>
    IVertexBuffer CreateVertexBuffer(
        float[] vertices);

    /// <summary>
    /// Draws triangles from the specified vertex buffer.
    /// </summary>
    /// <param name="vertexBuffer">
    /// Vertex buffer with the geometry to draw.
    /// </param>
    void DrawTriangles(
        IVertexBuffer vertexBuffer);
}