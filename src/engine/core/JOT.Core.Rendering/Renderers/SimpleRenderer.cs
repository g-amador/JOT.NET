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
    /// Uploads the specified RGBA pixel data to the graphics device
    /// as a 2D texture.
    /// </summary>
    /// <param name="width">Texture width in pixels.</param>
    /// <param name="height">Texture height in pixels.</param>
    /// <param name="pixels">
    /// RGBA pixel data. The length must be exactly
    /// <c>width * height * 4</c> bytes.
    /// </param>
    /// <returns>
    /// The uploaded texture.
    /// </returns>
    public ITexture2D CreateTexture(
        int width,
        int height,
        byte[] pixels)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                width,
                "Texture width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                height,
                "Texture height must be positive.");
        }

        ArgumentNullException.ThrowIfNull(pixels);

        return _graphics.CreateTexture(
            width,
            height,
            pixels);
    }

    /// <summary>
    /// Uploads the specified cube map faces to the graphics device
    /// as a cube map texture.
    ///
    /// The faces are uploaded in the OpenGL cube map order:
    /// positive X, negative X, positive Y, negative Y, positive Z,
    /// negative Z.
    /// </summary>
    /// <param name="width">Face width in pixels.</param>
    /// <param name="height">Face height in pixels.</param>
    /// <param name="faces">
    /// Face pixel data in cube map order. Every entry holds exactly
    /// <c>width * height * 4</c> bytes of RGBA data.
    /// </param>
    /// <returns>
    /// The uploaded cube map texture.
    /// </returns>
    public ITextureCubeMap CreateTextureCubeMap(
        int width,
        int height,
        byte[][] faces)
    {
        ArgumentNullException.ThrowIfNull(faces);

        return _graphics.CreateTextureCubeMap(
            width,
            height,
            faces);
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

    /// <summary>
    /// Draws triangles from the specified vertex buffer, sampling the
    /// specified texture.
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
    /// <param name="texture">
    /// Texture sampled at the sampler uniform.
    /// </param>
    /// <param name="samplerName">
    /// Name of the sampler2D uniform receiving the texture.
    /// </param>
    public void DrawTriangles(
        IVertexBuffer vertexBuffer,
        IShaderProgram shaderProgram,
        string uniformName,
        Matrix4x4 transform,
        ITexture2D texture,
        string samplerName)
    {
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        ArgumentNullException.ThrowIfNull(shaderProgram);
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentException.ThrowIfNullOrWhiteSpace(samplerName);

        shaderProgram.Use();
        shaderProgram.SetUniform(samplerName, 0);

        texture.Bind(0);

        shaderProgram.SetUniform(
            uniformName,
            transform);

        _graphics.DrawTriangles(vertexBuffer);
    }

    /// <inheritdoc cref="IGraphicsContext.CreateShadowRenderTarget"/>
    /// <param name="size">
    /// Depth texture size in pixels (square).
    /// </param>
    public IShadowRenderTarget CreateShadowRenderTarget(
        int size)
    {
        return _graphics.CreateShadowRenderTarget(size);
    }

    /// <summary>
    /// Sets the viewport region of the current render target.
    /// </summary>
    /// <param name="width">Viewport width in pixels.</param>
    /// <param name="height">Viewport height in pixels.</param>
    public void SetViewport(
        int width,
        int height)
    {
        _graphics.SetViewport(
            width,
            height);
    }

    /// <summary>
    /// Draws triangles from the specified vertex buffer, sampling the
    /// specified texture and supplying the view-projection and model
    /// transforms as separate uniforms so shaders can compute further
    /// world-space values such as shadow coordinates.
    /// </summary>
    /// <param name="vertexBuffer">
    /// Vertex buffer with the geometry to draw.
    /// </param>
    /// <param name="shaderProgram">
    /// Shader program used to draw the geometry.
    /// </param>
    /// <param name="viewProjectionUniformName">
    /// Name of the matrix uniform receiving the view-projection
    /// transform.
    /// </param>
    /// <param name="viewProjection">
    /// View-projection transform applied to the world geometry.
    /// </param>
    /// <param name="modelUniformName">
    /// Name of the matrix uniform receiving the model transform.
    /// </param>
    /// <param name="model">
    /// World transform of the geometry.
    /// </param>
    /// <param name="texture">
    /// Texture sampled at the sampler uniform.
    /// </param>
    /// <param name="samplerName">
    /// Name of the sampler2D uniform receiving the texture.
    /// </param>
    public void DrawTriangles(
        IVertexBuffer vertexBuffer,
        IShaderProgram shaderProgram,
        string viewProjectionUniformName,
        Matrix4x4 viewProjection,
        string modelUniformName,
        Matrix4x4 model,
        ITexture2D texture,
        string samplerName)
    {
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        ArgumentNullException.ThrowIfNull(shaderProgram);
        ArgumentException.ThrowIfNullOrWhiteSpace(viewProjectionUniformName);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelUniformName);
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentException.ThrowIfNullOrWhiteSpace(samplerName);

        shaderProgram.Use();
        shaderProgram.SetUniform(
            viewProjectionUniformName,
            viewProjection);
        shaderProgram.SetUniform(
            modelUniformName,
            model);
        shaderProgram.SetUniform(samplerName, 0);

        texture.Bind(0);

        _graphics.DrawTriangles(vertexBuffer);
    }

    /// <summary>
    /// Draws triangles from the specified vertex buffer, sampling the
    /// specified cube map texture.
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
    /// <param name="texture">
    /// Cube map texture sampled at the sampler uniform.
    /// </param>
    /// <param name="samplerName">
    /// Name of the samplerCube uniform receiving the texture.
    /// </param>
    public void DrawTriangles(
        IVertexBuffer vertexBuffer,
        IShaderProgram shaderProgram,
        string uniformName,
        Matrix4x4 transform,
        ITextureCubeMap texture,
        string samplerName)
    {
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        ArgumentNullException.ThrowIfNull(shaderProgram);
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentException.ThrowIfNullOrWhiteSpace(samplerName);

        shaderProgram.Use();
        shaderProgram.SetUniform(samplerName, 0);

        texture.Bind(0);

        shaderProgram.SetUniform(
            uniformName,
            transform);

        _graphics.DrawTriangles(vertexBuffer);
    }
}
