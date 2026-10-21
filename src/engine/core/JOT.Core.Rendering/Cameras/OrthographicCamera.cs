using System.Numerics;

namespace JOT.Core.Rendering.Cameras;

/// <summary>
/// Represents an orthographic camera.
/// </summary>
public sealed class OrthographicCamera : Camera
{
    /// <summary>
    /// Gets or sets the viewport width.
    /// </summary>
    public float Width { get; set; } = 10.0f;

    /// <summary>
    /// Gets or sets the viewport height.
    /// </summary>
    public float Height { get; set; } = 10.0f;

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
        Matrix4x4.CreateOrthographic(
            Width,
            Height,
            NearPlane,
            FarPlane);
}