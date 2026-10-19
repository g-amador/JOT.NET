using System.Numerics;

namespace JOT.Core.Math.Geometry;

/// <summary>
/// Represents a plane in three-dimensional space.
/// </summary>
public readonly struct Plane
{
    /// <summary>
    /// Gets the normalized plane normal.
    /// </summary>
    public Vector3 Normal { get; }

    /// <summary>
    /// Gets the plane distance from the origin.
    /// </summary>
    public float Distance { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Plane"/> structure.
    /// </summary>
    /// <param name="normal">
    /// Plane normal.
    /// </param>
    /// <param name="distance">
    /// Plane distance from the origin.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the normal vector is zero.
    /// </exception>
    public Plane(
        Vector3 normal,
        float distance)
    {
        if (normal == Vector3.Zero)
        {
            throw new ArgumentException(
                "Plane normal cannot be zero.",
                nameof(normal));
        }

        Normal = Vector3.Normalize(normal);
        Distance = distance;
    }

    /// <summary>
    /// Initializes a plane from a point and a normal.
    /// </summary>
    /// <param name="point">
    /// A point located on the plane.
    /// </param>
    /// <param name="normal">
    /// The plane normal.
    /// </param>
    public Plane(
        Vector3 point,
        Vector3 normal)
    {
        if (normal == Vector3.Zero)
        {
            throw new ArgumentException(
                "Plane normal cannot be zero.",
                nameof(normal));
        }

        Normal = Vector3.Normalize(normal);
        Distance = -Vector3.Dot(Normal, point);
    }

    /// <summary>
    /// Computes the signed distance from the plane to a point.
    /// </summary>
    /// <param name="point">
    /// Point to evaluate.
    /// </param>
    /// <returns>
    /// Signed distance from the point to the plane.
    /// </returns>
    public float GetDistanceToPoint(
        Vector3 point)
    {
        return Vector3.Dot(Normal, point) + Distance;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Normal: {Normal}, Distance: {Distance}";
    }
}