using Silk.NET.OpenGL;

namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Silk.NET OpenGL graphics context.
/// </summary>
public sealed class SilkGraphicsContext : IGraphicsContext
{
    private readonly GL _gl;

    public SilkGraphicsContext(GL gl)
    {
        _gl = gl;
    }

    /// <inheritdoc/>
    public void Clear(
        float red,
        float green,
        float blue,
        float alpha)
    {
        _gl.ClearColor(
            red,
            green,
            blue,
            alpha);

        _gl.Clear(
            ClearBufferMask.ColorBufferBit);
    }

    /// <inheritdoc/>
    public IShaderProgram CreateShaderProgram(
        string vertexShaderSource,
        string fragmentShaderSource)
    {
        var vertexShader = CompileShader(
            ShaderType.VertexShader,
            vertexShaderSource);

        var fragmentShader = CompileShader(
            ShaderType.FragmentShader,
            fragmentShaderSource);

        var program = _gl.CreateProgram();

        _gl.AttachShader(program, vertexShader);
        _gl.AttachShader(program, fragmentShader);
        _gl.LinkProgram(program);

        if (_gl.GetProgram(program, ProgramPropertyARB.LinkStatus) == 0)
        {
            throw new InvalidOperationException(
                $"Failed to link shader program: {_gl.GetProgramInfoLog(program)}");
        }

        return new SilkShaderProgram(_gl, program);
    }

    /// <inheritdoc/>
    public IVertexBuffer CreateVertexBuffer(
        float[] vertices)
    {
        return new SilkVertexBuffer(_gl, vertices);
    }

    /// <inheritdoc/>
    public void DrawTriangles(
        IVertexBuffer vertexBuffer)
    {
        if (vertexBuffer is not SilkVertexBuffer silkVertexBuffer)
        {
            throw new ArgumentException(
                "Unsupported vertex buffer type.",
                nameof(vertexBuffer));
        }

        silkVertexBuffer.Use();

        _gl.DrawArrays(
            PrimitiveType.Triangles,
            0,
            (uint)silkVertexBuffer.VertexCount);
    }

    private uint CompileShader(
        ShaderType type,
        string source)
    {
        var shader = _gl.CreateShader(type);

        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        if (_gl.GetShader(shader, ShaderParameterName.CompileStatus) == 0)
        {
            throw new InvalidOperationException(
                $"Failed to compile {type} shader: {_gl.GetShaderInfoLog(shader)}");
        }

        return shader;
    }
}