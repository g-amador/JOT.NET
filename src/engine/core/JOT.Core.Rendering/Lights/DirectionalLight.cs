using System.Numerics;

namespace JOT.Core.Rendering.Lights;

/// <summary>
/// Represents a directional light shining on a scene.
/// </summary>
public sealed class DirectionalLight
{
    /// <summary>
    /// Gets or sets the direction the light travels: from the light
    /// position toward the scene.
    /// </summary>
    public Vector3 Direction { get; set; }
        = new(0.0f, -1.0f, 0.0f);

    /// <summary>
    /// Gets or sets the light color.
    /// </summary>
    public Vector3 Color { get; set; }
        = Vector3.One;

    /// <summary>
    /// Gets or sets the ambient light contribution, typically between
    /// 0 and 1.
    /// </summary>
    public float AmbientStrength { get; set; }
        = 0.3f;

    /// <summary>
    /// Gets the normalized direction from scene surfaces toward the
    /// light.
    /// </summary>
    public Vector3 DirectionToLight => Vector3.Normalize(-Direction);

    /// <summary>
    /// Builds the view-projection matrix used for shadow map rendering
    /// of a scene centered on the origin.
    /// </summary>
    /// <param name="extent">
    /// Half extent of the square orthographic shadow volume in world
    /// units around the scene center.
    /// </param>
    /// <param name="distance">
    /// Distance from the scene center toward the light along the light
    /// direction.
    /// </param>
    /// <param name="nearPlane">Near clipping plane distance.</param>
    /// <param name="farPlane">Far clipping plane distance.</param>
    /// <returns>The light view-projection matrix.</returns>
    public Matrix4x4 CreateShadowViewProjection(
        float extent,
        float distance,
        float nearPlane,
        float farPlane)
    {
        if (extent <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(extent),
                extent,
                "Shadow extent must be positive.");
        }

        if (distance <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(distance),
                distance,
                "Shadow distance must be positive.");
        }

        if (nearPlane <= 0.0f || farPlane <= nearPlane)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nearPlane),
                nearPlane,
                "Shadow near plane must be positive and below the far plane.");
        }

        var direction = Vector3.Normalize(Direction);

        var eye = -direction * distance;

        var up = MathF.Abs(direction.Y) > 0.99f
            ? Vector3.UnitX
            : Vector3.UnitY;

        var view = Matrix4x4.CreateLookAt(
            eye,
            Vector3.Zero,
            up);

        var projection = Matrix4x4.CreateOrthographic(
            extent * 2.0f,
            extent * 2.0f,
            nearPlane,
            farPlane);

        return view * projection;
    }
}
