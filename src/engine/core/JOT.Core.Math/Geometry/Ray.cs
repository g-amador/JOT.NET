using System.Numerics;

namespace JOT.Core.Math.Geometry;

/// <summary>
/// Represents a ray in three-dimensional space.
/// </summary>
public readonly struct Ray
{
    /// <summary>
    /// Gets the origin of the ray.
    /// </summary>
    public Vector3 Origin { get; }

    /// <summary>
    /// Gets the normalized direction of the ray.
    /// </summary>
    public Vector3 Direction { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ray"/> structure.
    /// </summary>
    /// <param name="origin">
    /// The starting point of the ray.
    /// </param>
    /// <param name="direction">
    /// The direction of the ray.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the direction vector is zero.
    /// </exception>
    public Ray(
        Vector3 origin,
        Vector3 direction)
    {
        if (direction == Vector3.Zero)
        {
            throw new ArgumentException(
                "Ray direction cannot be zero.",
                nameof(direction));
        }

        Origin = origin;
        Direction = Vector3.Normalize(direction);
    }

    /// <summary>
    /// Returns a point along the ray at the specified distance.
    /// </summary>
    /// <param name="distance">
    /// Distance from the ray origin.
    /// </param>
    /// <returns>
    /// A point on the ray.
    /// </returns>
    public Vector3 GetPoint(float distance)
    {
        return Origin + (Direction * distance);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Origin: {Origin}, Direction: {Direction}";
    }
}