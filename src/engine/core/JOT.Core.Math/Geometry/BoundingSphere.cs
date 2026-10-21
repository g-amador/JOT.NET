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
    /// Creates a sphere containing all specified points.
    ///
    /// The sphere is approximated: the center is the average of the
    /// points and the radius reaches the farthest point from it.
    /// </summary>
    /// <param name="points">
    /// Positions to enclose.
    /// </param>
    /// <returns>
    /// A bounding sphere containing all points.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="points"/> is empty.
    /// </exception>
    public static BoundingSphere CreateFromPoints(
        ReadOnlySpan<Vector3> points)
    {
        if (points.Length == 0)
        {
            throw new ArgumentException(
                "At least one point is required.",
                nameof(points));
        }

        var center = Vector3.Zero;

        foreach (var point in points)
        {
            center += point;
        }

        center /= points.Length;

        var radiusSquared = 0.0f;

        foreach (var point in points)
        {
            var distanceSquared = Vector3.DistanceSquared(
                center,
                point);

            if (distanceSquared > radiusSquared)
            {
                radiusSquared = distanceSquared;
            }
        }

        return new BoundingSphere(
            center,
            MathF.Sqrt(radiusSquared));
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
    /// Transforms the sphere by the specified matrix.
    ///
    /// The radius is scaled by the largest axis length of the matrix so
    /// that the result always contains the transformed sphere; it may be
    /// conservative for non-uniform scaling or shearing.
    /// </summary>
    /// <param name="transform">
    /// Transform matrix to apply, in the form translation * rotation * scale.
    /// </param>
    /// <returns>
    /// The transformed bounding sphere.
    /// </returns>
    public BoundingSphere Transform(Matrix4x4 transform)
    {
        var scaleX = new Vector3(
            transform.M11,
            transform.M12,
            transform.M13).Length();

        var scaleY = new Vector3(
            transform.M21,
            transform.M22,
            transform.M23).Length();

        var scaleZ = new Vector3(
            transform.M31,
            transform.M32,
            transform.M33).Length();

        return new BoundingSphere(
            Vector3.Transform(Center, transform),
            Radius * MathF.Max(scaleX, MathF.Max(scaleY, scaleZ)));
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