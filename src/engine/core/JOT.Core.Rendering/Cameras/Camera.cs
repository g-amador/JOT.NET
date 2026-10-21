using JOT.Core.Math.Transformations;
using System.Numerics;

namespace JOT.Core.Rendering.Cameras;

/// <summary>
/// Provides common camera functionality.
/// </summary>
public abstract class Camera : ICamera
{
    /// <inheritdoc />
    public Transform Transform { get; }
        = new();

    /// <inheritdoc />
    public Matrix4x4 ViewMatrix =>
        Matrix4x4.CreateLookAt(
            Transform.Position,
            Transform.Position + Transform.Forward,
            Transform.Up);

    /// <inheritdoc />
    public abstract Matrix4x4 ProjectionMatrix
    {
        get;
    }
}