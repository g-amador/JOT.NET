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
    /// attribute 1 is an RGB color, both as three floats per vertex,
    /// followed by attribute 2, a UV coordinate as two floats
    /// per vertex.
    /// </summary>
    /// <param name="vertices">
    /// Interleaved vertex data. The length must be a multiple of eight.
    /// </param>
    /// <returns>
    /// The uploaded vertex buffer.
    /// </returns>
    IVertexBuffer CreateVertexBuffer(
        float[] vertices);

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
    ITexture2D CreateTexture(
        int width,
        int height,
        byte[] pixels);

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
    ITextureCubeMap CreateTextureCubeMap(
        int width,
        int height,
        byte[][] faces);

    /// <summary>
    /// Creates a shadow render target: a framebuffer holding only a
    /// depth attachment of the specified size, used as the depth pass
    /// target for shadow map rendering and as the sampled shadow map.
    /// </summary>
    /// <param name="size">
    /// Depth texture size in pixels (square).
    /// </param>
    /// <returns>
    /// The shadow render target.
    /// </returns>
    IShadowRenderTarget CreateShadowRenderTarget(
        int size);

    /// <summary>
    /// Sets the viewport region of the current render target.
    /// </summary>
    /// <param name="width">Viewport width in pixels.</param>
    /// <param name="height">Viewport height in pixels.</param>
    void SetViewport(
        int width,
        int height);

    /// <summary>
    /// Draws triangles from the specified vertex buffer.
    /// </summary>
    /// <param name="vertexBuffer">
    /// Vertex buffer with the geometry to draw.
    /// </param>
    void DrawTriangles(
        IVertexBuffer vertexBuffer);

    /// <summary>
    /// Draws line segments from the specified vertex buffer: two
    /// consecutive vertices per segment, sharing the vertex layout of
    /// triangle buffers.
    /// </summary>
    /// <param name="vertexBuffer">
    /// Vertex buffer with the line geometry to draw.
    /// </param>
    void DrawLines(
        IVertexBuffer vertexBuffer);

    /// <summary>
    /// Sets whether triangle geometry renders blended over the existing
    /// framebuffer pixels: enabled mixes incoming fragments with the
    /// framebuffer by their alpha for translucent overlays such as
    /// on-screen text; disabled writes every fragment fully opaque.
    /// Applies to subsequent draws until toggled again.
    /// </summary>
    /// <param name="enabled">
    /// Whether alpha blending runs (true) or pixels overwrite (false).
    /// </param>
    void SetBlending(
        bool enabled);

    /// <summary>
    /// Sets whether fragment depth testing runs: enabled draws accept
    /// only fragments in front of stored depth; disabled draws overlay
    /// everything, used by two-dimensional interfaces like the HUD.
    /// Applies to subsequent draws until toggled again. Reactivating
    /// the depth test restores the standard less-than comparison.
    /// </summary>
    /// <param name="enabled">
    /// Whether depth testing runs (true) or fragments bypass it
    /// (false).
    /// </param>
    void SetDepthTest(
        bool enabled);

    /// <summary>
    /// Toggles wireframe rendering of triangle geometry: enabled draws
    /// only the triangle edges as lines, disabled renders filled
    /// triangles. Applies to subsequent triangle draws until toggled.
    /// </summary>
    /// <param name="enabled">
    /// Whether triangles render as wireframe (true) or filled (false).
    /// </param>
    void SetWireframe(
        bool enabled);
}