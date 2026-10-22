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

        // System.Numerics matrices use row-vector semantics (v * M) with
        // row-major storage. OpenGL applies column vectors (M^T * v), so
        // uploading with transpose=false makes GL read the array as
        // column-major, yielding the transposed matrix the shaders need.
        Span<float> values =
        [
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44,
        ];

        _gl.UniformMatrix4(
            location,
            false,
            values);
    }

    /// <inheritdoc/>
    public void SetUniform(
        string name,
        Vector3 value)
    {
        var location = _gl.GetUniformLocation(_program, name);

        if (location < 0)
        {
            throw new InvalidOperationException(
                $"Uniform '{name}' does not exist in the shader program.");
        }

        _gl.Uniform3(
            location,
            value.X,
            value.Y,
            value.Z);
    }

    /// <inheritdoc/>
    public void SetUniform(
        string name,
        float value)
    {
        var location = _gl.GetUniformLocation(_program, name);

        if (location < 0)
        {
            throw new InvalidOperationException(
                $"Uniform '{name}' does not exist in the shader program.");
        }

        _gl.Uniform1(location, value);
    }

    /// <inheritdoc/>
    public void SetUniform(
        string name,
        int value)
    {
        var location = _gl.GetUniformLocation(_program, name);

        if (location < 0)
        {
            throw new InvalidOperationException(
                $"Uniform '{name}' does not exist in the shader program.");
        }

        _gl.Uniform1(location, value);
    }
}
