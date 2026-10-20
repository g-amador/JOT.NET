using System.Numerics;

namespace JOT.Core.Rendering.Cameras;

/// <summary>
/// Represents a perspective camera.
/// </summary>
public sealed class PerspectiveCamera : Camera
{
    /// <summary>
    /// Gets or sets the field of view in radians.
    /// </summary>
    public float FieldOfView { get; set; }
        = MathF.PI / 4.0f;

    /// <summary>
    /// Gets or sets the aspect ratio.
    /// </summary>
    public float AspectRatio { get; set; }
        = 16.0f / 9.0f;

    /// <summary>
    /// Gets or sets the near clipping plane.
    /// </summary>
    public float NearPlane { get; set; }
        = 0.1f;

    /// <summary>
    /// Gets or sets the far clipping plane.
    /// </summary>
    public float FarPlane { get; set; }
        = 1000.0f;

    /// <inheritdoc />
    public override Matrix4x4 ProjectionMatrix =>
        Matrix4x4.CreatePerspectiveFieldOfView(
            FieldOfView,
            AspectRatio,
            NearPlane,
            FarPlane);
}