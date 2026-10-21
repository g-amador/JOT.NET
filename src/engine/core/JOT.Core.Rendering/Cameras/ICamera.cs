using JOT.Core.Math.Transformations;
using System.Numerics;

namespace JOT.Core.Rendering.Cameras;

/// <summary>
/// Defines a rendering camera.
/// </summary>
public interface ICamera
{
    /// <summary>
    /// Gets the camera transform.
    /// </summary>
    Transform Transform { get; }

    /// <summary>
    /// Gets the view matrix.
    /// </summary>
    Matrix4x4 ViewMatrix { get; }

    /// <summary>
    /// Gets the projection matrix.
    /// </summary>
    Matrix4x4 ProjectionMatrix { get; }
}