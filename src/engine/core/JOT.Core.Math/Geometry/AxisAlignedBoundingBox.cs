using System.Numerics;

namespace JOT.Core.Math.Geometry;

/// <summary>
/// Represents an axis-aligned bounding box (AABB).
/// </summary>
public readonly struct AxisAlignedBoundingBox
{
    /// <summary>
    /// Gets the minimum corner of the box.
    /// </summary>
    public Vector3 Min { get; }

    /// <summary>
    /// Gets the maximum corner of the box.
    /// </summary>
    public Vector3 Max { get; }

    /// <summary>
    /// Gets the center of the box.
    /// </summary>
    public Vector3 Center => (Min + Max) * 0.5f;

    /// <summary>
    /// Gets the size of the box.
    /// </summary>
    public Vector3 Size => Max - Min;

    /// <summary>
    /// Gets the half-extents of the box.
    /// </summary>
    public Vector3 Extents => Size * 0.5f;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AxisAlignedBoundingBox"/> structure.
    /// </summary>
    /// <param name="min">
    /// Minimum corner of the box.
    /// </param>
    /// <param name="max">
    /// Maximum corner of the box.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when any component of <paramref name="min"/>
    /// is greater than the corresponding component of
    /// <paramref name="max"/>.
    /// </exception>
    public AxisAlignedBoundingBox(
        Vector3 min,
        Vector3 max)
    {
        if (min.X > max.X ||
            min.Y > max.Y ||
            min.Z > max.Z)
        {
            throw new ArgumentException(
                "Minimum coordinates must be less than or equal to maximum coordinates.");
        }

        Min = min;
        Max = max;
    }

    /// <summary>
    /// Determines whether the specified point is contained within the box.
    /// </summary>
    /// <param name="point">
    /// Point to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if the point is inside the box; otherwise <c>false</c>.
    /// </returns>
    public bool Contains(Vector3 point)
    {
        return point.X >= Min.X &&
               point.X <= Max.X &&
               point.Y >= Min.Y &&
               point.Y <= Max.Y &&
               point.Z >= Min.Z &&
               point.Z <= Max.Z;
    }

    /// <summary>
    /// Transforms the box by the specified matrix and returns the smallest
    /// axis-aligned bounding box that contains the result.
    ///
    /// The returned box always contains the transformed box; it may be
    /// larger than necessary when the matrix rotates or shears the box.
    /// </summary>
    /// <param name="transform">
    /// Transform matrix to apply, in the form translation * rotation * scale.
    /// </param>
    /// <returns>
    /// The transformed axis-aligned bounding box.
    /// </returns>
    public AxisAlignedBoundingBox Transform(Matrix4x4 transform)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        for (var i = 0; i < 8; i++)
        {
            var corner = new Vector3(
                (i & 1) == 0 ? Min.X : Max.X,
                (i & 2) == 0 ? Min.Y : Max.Y,
                (i & 4) == 0 ? Min.Z : Max.Z);

            corner = Vector3.Transform(
                corner,
                transform);

            min = Vector3.Min(min, corner);
            max = Vector3.Max(max, corner);
        }

        return new AxisAlignedBoundingBox(min, max);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Min: {Min}, Max: {Max}";
    }
}