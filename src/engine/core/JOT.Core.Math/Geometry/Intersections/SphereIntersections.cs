using System.Numerics;

namespace JOT.Core.Math.Geometry.Intersections;

/// <summary>
/// Provides bounding sphere intersection tests.
/// </summary>
public static class SphereIntersections
{
    /// <summary>
    /// Determines whether two spheres intersect.
    /// </summary>
    /// <param name="first">
    /// First sphere.
    /// </param>
    /// <param name="second">
    /// Second sphere.
    /// </param>
    /// <returns>
    /// <c>true</c> if the spheres intersect; otherwise <c>false</c>.
    /// </returns>
    public static bool Intersects(
        BoundingSphere first,
        BoundingSphere second)
    {
        var radius =
            first.Radius + second.Radius;

        return Vector3.DistanceSquared(
            first.Center,
            second.Center)
            <= radius * radius;
    }
}