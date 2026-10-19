using System.Numerics;

namespace JOT.Core.Math.Geometry;

/// <summary>
/// Represents an oriented bounding box (OBB).
/// </summary>
public readonly struct OrientedBoundingBox
{
    /// <summary>
    /// Gets the center of the box.
    /// </summary>
    public Vector3 Center { get; }

    /// <summary>
    /// Gets the half-extents of the box.
    /// </summary>
    public Vector3 Extents { get; }

    /// <summary>
    /// Gets the box orientation.
    /// </summary>
    public Quaternion Rotation { get; }

    /// <summary>
    /// Gets the local X axis.
    /// </summary>
    public Vector3 Right =>
        Vector3.Transform(
            Vector3.UnitX,
            Rotation);

    /// <summary>
    /// Gets the local Y axis.
    /// </summary>
    public Vector3 Up =>
        Vector3.Transform(
            Vector3.UnitY,
            Rotation);

    /// <summary>
    /// Gets the local Z axis.
    /// </summary>
    public Vector3 Forward =>
        Vector3.Transform(
            Vector3.UnitZ,
            Rotation);

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="OrientedBoundingBox"/> structure.
    /// </summary>
    /// <param name="center">
    /// Center of the box.
    /// </param>
    /// <param name="extents">
    /// Half-extents of the box.
    /// </param>
    /// <param name="rotation">
    /// Box orientation.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any extent component is negative.
    /// </exception>
    public OrientedBoundingBox(
        Vector3 center,
        Vector3 extents,
        Quaternion rotation)
    {
        if (extents.X < 0.0f ||
            extents.Y < 0.0f ||
            extents.Z < 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(extents),
                "Extents cannot be negative.");
        }

        Center = center;
        Extents = extents;
        Rotation = Quaternion.Normalize(rotation);
    }

    /// <summary>
    /// Gets the full size of the box.
    /// </summary>
    public Vector3 Size => Extents * 2.0f;

    /// <inheritdoc />
    public override string ToString()
    {
        return
            $"Center: {Center}, " +
            $"Extents: {Extents}, " +
            $"Rotation: {Rotation}";
    }
}