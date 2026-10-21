namespace JOT.Core.Math.Geometry.Intersections;

/// <summary>
/// Provides bounding box intersection tests.
/// </summary>
public static class BoxIntersections
{
    /// <summary>
    /// Determines whether two axis-aligned bounding boxes intersect.
    /// </summary>
    /// <param name="first">
    /// First box.
    /// </param>
    /// <param name="second">
    /// Second box.
    /// </param>
    /// <returns>
    /// <c>true</c> if the boxes intersect; otherwise <c>false</c>.
    /// </returns>
    public static bool Intersects(
        AxisAlignedBoundingBox first,
        AxisAlignedBoundingBox second)
    {
        return
            first.Min.X <= second.Max.X &&
            first.Max.X >= second.Min.X &&

            first.Min.Y <= second.Max.Y &&
            first.Max.Y >= second.Min.Y &&

            first.Min.Z <= second.Max.Z &&
            first.Max.Z >= second.Min.Z;
    }
}