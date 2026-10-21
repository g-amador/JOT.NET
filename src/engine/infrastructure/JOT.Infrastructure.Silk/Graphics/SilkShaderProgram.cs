using Silk.NET.OpenGL;
using System.Numerics;

namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Silk.NET OpenGL implementation of a shader program.
/// </summary>
internal sealed class SilkShaderProgram : IShaderProgram
{
    private readonly GL _gl;

    private readonly uint _program;

    /// <summary>
    /// Initializes a new instance of the <see cref="SilkShaderProgram"/> class.
    /// </summary>
    /// <param name="gl">OpenGL API instance.</param>
    /// <param name="program">Compiled shader program name.</param>
    public SilkShaderProgram(
        GL gl,
        uint program)
    {
        ArgumentNullException.ThrowIfNull(gl);

        _gl = gl;
        _program = program;
    }

    /// <inheritdoc/>
    public void Use()
    {
        _gl.UseProgram(_program);
    }

    /// <inheritdoc/>
    public void SetUniform(
        string name,
        Matrix4x4 matrix)
    {
        var location = _gl.GetUniformLocation(_program, name);

        if (location < 0)
        {
            throw new InvalidOperationException(
                $"Uniform '{name}' does not exist in the shader program.");
        }

        // System.Numerics matrices are stored row-major, so the values are
        // transposed on upload to match OpenGL column-major layout.
        Span<float> values =
        [
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44,
        ];

        _gl.UniformMatrix4(
            location,
            true,
            values);
    }
}
