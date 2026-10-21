using JOT.Core.Rendering.Meshes;
using JOT.Infrastructure.Silk.Graphics;
using System.Numerics;

namespace JOT.Core.Rendering.Renderers;

/// <summary>
/// Renders geometry through a graphics context.
///
/// Built on top of the Infrastructure glue contracts; it contains the
/// first engine rendering logic and is expanded as the Core phase
/// progresses.
/// </summary>
public sealed class SimpleRenderer
{
    private readonly IGraphicsContext _graphics;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleRenderer"/> class.
    /// </summary>
    /// <param name="graphics">
    /// Graphics context used for rendering.
    /// </param>
    public SimpleRenderer(IGraphicsContext graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);

        _graphics = graphics;
    }

    /// <summary>
    /// Clears the render target using the specified color.
    /// </summary>
    /// <param name="red">Red component (0-1).</param>
    /// <param name="green">Green component (0-1).</param>
    /// <param name="blue">Blue component (0-1).</param>
    /// <param name="alpha">Alpha component (0-1).</param>
    public void Clear(
        float red,
        float green,
        float blue,
        float alpha)
    {
        _graphics.Clear(
            red,
            green,
            blue,
            alpha);
    }

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
    public IShaderProgram CreateShaderProgram(
        string vertexShaderSource,
        string fragmentShaderSource)
    {
        return _graphics.CreateShaderProgram(
            vertexShaderSource,
            fragmentShaderSource);
    }

    /// <summary>
    /// Uploads the mesh vertex data to the graphics device.
    /// </summary>
    /// <param name="mesh">
    /// Mesh with the vertex data to upload.
    /// </param>
    /// <returns>
    /// The uploaded vertex buffer.
    /// </returns>
    public IVertexBuffer CreateVertexBuffer(Mesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        return _graphics.CreateVertexBuffer(mesh.Vertices);
    }

    /// <summary>
    /// Draws triangles from the specified vertex buffer.
    /// </summary>
    /// <param name="vertexBuffer">
    /// Vertex buffer with the geometry to draw.
    /// </param>
    /// <param name="shaderProgram">
    /// Shader program used to draw the geometry.
    /// </param>
    /// <param name="uniformName">
    /// Name of the matrix uniform that receives the transform.
    /// </param>
    /// <param name="transform">
    /// Transform matrix applied to the geometry.
    /// </param>
    public void DrawTriangles(
        IVertexBuffer vertexBuffer,
        IShaderProgram shaderProgram,
        string uniformName,
        Matrix4x4 transform)
    {
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        ArgumentNullException.ThrowIfNull(shaderProgram);

        shaderProgram.Use();
        shaderProgram.SetUniform(
            uniformName,
            transform);

        _graphics.DrawTriangles(vertexBuffer);
    }
}
