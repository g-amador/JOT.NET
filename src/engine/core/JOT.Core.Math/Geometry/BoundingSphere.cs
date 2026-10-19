using System.Numerics;

namespace JOT.Core.Math.Geometry;

/// <summary>
/// Represents a bounding sphere in three-dimensional space.
/// </summary>
public readonly struct BoundingSphere
{
    /// <summary>
    /// Gets the center of the sphere.
    /// </summary>
    public Vector3 Center { get; }

    /// <summary>
    /// Gets the radius of the sphere.
    /// </summary>
    public float Radius { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingSphere"/> structure.
    /// </summary>
    /// <param name="center">
    /// Sphere center.
    /// </param>
    /// <param name="radius">
    /// Sphere radius.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the radius is negative.
    /// </exception>
    public BoundingSphere(
        Vector3 center,
        float radius)
    {
        if (radius < 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                "Sphere radius cannot be negative.");
        }

        Center = center;
        Radius = radius;
    }

    /// <summary>
    /// Determines whether the specified point is inside the sphere.
    /// </summary>
    /// <param name="point">
    /// Point to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if the point is inside the sphere; otherwise <c>false</c>.
    /// </returns>
    public bool Contains(Vector3 point)
    {
        var radiusSquared = Radius * Radius;

        return Vector3.DistanceSquared(
            Center,
            point) <= radiusSquared;
    }

    /// <summary>
    /// Gets the sphere diameter.
    /// </summary>
    public float Diameter => Radius * 2.0f;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Center: {Center}, Radius: {Radius}";
    }
}