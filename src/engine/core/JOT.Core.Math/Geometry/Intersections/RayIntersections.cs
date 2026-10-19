using System.Numerics;

namespace JOT.Core.Math.Geometry.Intersections;

/// <summary>
/// Provides ray intersection tests.
/// </summary>
public static class RayIntersections
{
    /// <summary>
    /// Determines whether a ray intersects a plane.
    /// </summary>
    /// <param name="ray">
    /// Ray to test.
    /// </param>
    /// <param name="plane">
    /// Plane to test.
    /// </param>
    /// <param name="distance">
    /// Distance from the ray origin to the intersection point.
    /// </param>
    /// <returns>
    /// <c>true</c> if an intersection occurs; otherwise <c>false</c>.
    /// </returns>
    public static bool Intersects(
        Ray ray,
        Plane plane,
        out float distance)
    {
        const float epsilon = 0.00001f;

        var denominator =
            Vector3.Dot(
                plane.Normal,
                ray.Direction);

        if (System.MathF.Abs(denominator) < epsilon)
        {
            distance = 0.0f;
            return false;
        }

        distance =
            -(Vector3.Dot(
                plane.Normal,
                ray.Origin) + plane.Distance)
            / denominator;

        return distance >= 0.0f;
    }

    /// <summary>
    /// Determines whether a ray intersects a sphere.
    /// </summary>
    /// <param name="ray">
    /// Ray to test.
    /// </param>
    /// <param name="sphere">
    /// Sphere to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if an intersection occurs; otherwise <c>false</c>.
    /// </returns>
    public static bool Intersects(
        Ray ray,
        BoundingSphere sphere)
    {
        var offset =
            sphere.Center - ray.Origin;

        var projection =
            Vector3.Dot(
                offset,
                ray.Direction);

        var distanceSquared =
            Vector3.Dot(offset, offset)
            - projection * projection;

        return distanceSquared
            <= sphere.Radius * sphere.Radius;
    }
}