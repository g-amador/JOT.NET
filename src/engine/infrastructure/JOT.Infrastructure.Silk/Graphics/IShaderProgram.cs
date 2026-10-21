using System.Numerics;

namespace JOT.Infrastructure.Silk.Graphics;

/// <summary>
/// Represents a compiled shader program.
/// </summary>
public interface IShaderProgram
{
    /// <summary>
    /// Binds the shader program for subsequent rendering.
    /// </summary>
    void Use();

    /// <summary>
    /// Sets a matrix uniform on the shader program.
    /// </summary>
    /// <param name="name">
    /// Name of the uniform, as declared in the shader source.
    /// </param>
    /// <param name="matrix">
    /// Matrix value assigned to the uniform.
    /// </param>
    void SetUniform(
        string name,
        Matrix4x4 matrix);
}
