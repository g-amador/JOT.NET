using System.Numerics;

namespace JOT.Core.Math.Transformations;

/// <summary>
/// Represents a 3D transform consisting of position, rotation, and scale.
/// </summary>
public sealed class Transform
{
    /// <summary>
    /// Gets or sets the world position.
    /// </summary>
    public Vector3 Position { get; set; } = Vector3.Zero;

    /// <summary>
    /// Gets or sets the world rotation.
    /// </summary>
    public Quaternion Rotation { get; set; }
        = Quaternion.Identity;

    /// <summary>
    /// Gets or sets the local scale.
    /// </summary>
    public Vector3 Scale { get; set; }
        = Vector3.One;

    /// <summary>
    /// Gets the forward direction.
    /// </summary>
    public Vector3 Forward =>
        Vector3.Transform(
            -Vector3.UnitZ,
            Rotation);

    /// <summary>
    /// Gets the right direction.
    /// </summary>
    public Vector3 Right =>
        Vector3.Transform(
            Vector3.UnitX,
            Rotation);

    /// <summary>
    /// Gets the up direction.
    /// </summary>
    public Vector3 Up =>
        Vector3.Transform(
            Vector3.UnitY,
            Rotation);

    /// <summary>
    /// Gets the world transform matrix.
    /// </summary>
    public Matrix4x4 WorldMatrix =>
        Matrix4x4.CreateScale(Scale) *
        Matrix4x4.CreateFromQuaternion(Rotation) *
        Matrix4x4.CreateTranslation(Position);

    /// <summary>
    /// Resets the transform to its identity state.
    /// </summary>
    public void Reset()
    {
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
    }
}