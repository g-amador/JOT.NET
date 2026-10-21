using System.Numerics;

namespace JOT.Core.Math.Geometry.Intersections;

/// <summary>
/// Provides frustum intersection tests.
/// </summary>
public static class FrustumIntersections
{
    /// <summary>
    /// Determines whether a frustum intersects a bounding sphere.
    /// </summary>
    /// <param name="frustum">
    /// Frustum to test.
    /// </param>
    /// <param name="sphere">
    /// Sphere to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if the sphere is fully or partially inside the frustum;
    /// otherwise <c>false</c>.
    /// </returns>
    public static bool Intersects(
        Frustum frustum,
        BoundingSphere sphere)
    {
        return
            IsAbovePlane(frustum.Near, sphere) &&
            IsAbovePlane(frustum.Far, sphere) &&
            IsAbovePlane(frustum.Left, sphere) &&
            IsAbovePlane(frustum.Right, sphere) &&
            IsAbovePlane(frustum.Top, sphere) &&
            IsAbovePlane(frustum.Bottom, sphere);
    }

    /// <summary>
    /// Determines whether a frustum intersects an axis-aligned bounding box.
    /// </summary>
    /// <param name="frustum">
    /// Frustum to test.
    /// </param>
    /// <param name="box">
    /// Box to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if the box is fully or partially inside the frustum;
    /// otherwise <c>false</c>.
    /// </returns>
    public static bool Intersects(
        Frustum frustum,
        AxisAlignedBoundingBox box)
    {
        return
            IsAbovePlane(frustum.Near, box) &&
            IsAbovePlane(frustum.Far, box) &&
            IsAbovePlane(frustum.Left, box) &&
            IsAbovePlane(frustum.Right, box) &&
            IsAbovePlane(frustum.Top, box) &&
            IsAbovePlane(frustum.Bottom, box);
    }

    private static bool IsAbovePlane(
        Plane plane,
        BoundingSphere sphere)
    {
        return plane.GetDistanceToPoint(sphere.Center)
            >= -sphere.Radius;
    }

    private static bool IsAbovePlane(
        Plane plane,
        AxisAlignedBoundingBox box)
    {
        // Test the box corner farthest along the plane normal (the
        // "positive vertex"). If even that corner is outside the plane,
        // then the whole box is outside.
        var positive = new Vector3(
            plane.Normal.X >= 0.0f ? box.Max.X : box.Min.X,
            plane.Normal.Y >= 0.0f ? box.Max.Y : box.Min.Y,
            plane.Normal.Z >= 0.0f ? box.Max.Z : box.Min.Z);

        return plane.GetDistanceToPoint(positive) >= 0.0f;
    }
}
