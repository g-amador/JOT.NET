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
    /// Transforms the box by the specified matrix.
    ///
    /// The center and the three box axes are transformed individually and
    /// the extents are taken from the transformed axis lengths. When the
    /// matrix orients or scales the box non-uniformly, the result is a
    /// close-fitting box that still contains the original.
    /// </summary>
    /// <param name="transform">
    /// Transform matrix to apply, in the form translation * rotation * scale.
    /// </param>
    /// <returns>
    /// The transformed oriented bounding box.
    /// </returns>
    public OrientedBoundingBox Transform(Matrix4x4 transform)
    {
        var center = Vector3.Transform(
            Center,
            transform);

        var right = Vector3.TransformNormal(
            new Vector3(Extents.X, 0.0f, 0.0f),
            transform);

        var up = Vector3.TransformNormal(
            new Vector3(0.0f, Extents.Y, 0.0f),
            transform);

        var forward = Vector3.TransformNormal(
            new Vector3(0.0f, 0.0f, Extents.Z),
            transform);

        var extents = new Vector3(
            right.Length(),
            up.Length(),
            forward.Length());

        right = GetAxis(right, Rotation, Vector3.UnitX);
        up = GetAxis(up, Rotation, Vector3.UnitY);
        forward = GetAxis(forward, Rotation, Vector3.UnitZ);

        // Basis vector images are the matrix rows under row-vector
        // semantics, so the rebuilt rotation matrix holds the transformed
        // axes in its rows.
        var rotation = new Matrix4x4(
            right.X, right.Y, right.Z, 0.0f,
            up.X, up.Y, up.Z, 0.0f,
            forward.X, forward.Y, forward.Z, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f);

        return new OrientedBoundingBox(
            center,
            extents,
            Quaternion.CreateFromRotationMatrix(rotation));
    }

    private static Vector3 GetAxis(
        Vector3 axis,
        Quaternion fallbackRotation,
        Vector3 fallbackAxis)
    {
        if (axis.LengthSquared() > float.Epsilon)
        {
            return Vector3.Normalize(axis);
        }

        return Vector3.Transform(
            fallbackAxis,
            fallbackRotation);
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