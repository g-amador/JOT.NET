using System.Numerics;

namespace JOT.Core.Math.Geometry;
/// <summary>
/// Represents a view frustum defined by six clipping planes.
///
/// A frustum describes the visible volume of a camera and is commonly
/// used for visibility testing, culling, and spatial queries.
///
/// Plane normals are expected to face inward toward the frustum volume.
/// </summary>
public readonly struct Frustum
{
    /// <summary>
    /// Gets the near clipping plane.
    /// </summary>
    public Plane Near { get; }

    /// <summary>
    /// Gets the far clipping plane.
    /// </summary>
    public Plane Far { get; }

    /// <summary>
    /// Gets the left clipping plane.
    /// </summary>
    public Plane Left { get; }

    /// <summary>
    /// Gets the right clipping plane.
    /// </summary>
    public Plane Right { get; }

    /// <summary>
    /// Gets the top clipping plane.
    /// </summary>
    public Plane Top { get; }

    /// <summary>
    /// Gets the bottom clipping plane.
    /// </summary>
    public Plane Bottom { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Frustum"/> structure.
    /// </summary>
    /// <param name="near">
    /// Near clipping plane.
    /// </param>
    /// <param name="far">
    /// Far clipping plane.
    /// </param>
    /// <param name="left">
    /// Left clipping plane.
    /// </param>
    /// <param name="right">
    /// Right clipping plane.
    /// </param>
    /// <param name="top">
    /// Top clipping plane.
    /// </param>
    /// <param name="bottom">
    /// Bottom clipping plane.
    /// </param>
    public Frustum(
        Plane near,
        Plane far,
        Plane left,
        Plane right,
        Plane top,
        Plane bottom)
    {
        Near = near;
        Far = far;
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    /// <summary>
    /// Creates a frustum from a combined view-projection matrix.
    ///
    /// Pass the composite of a camera's view and projection matrices
    /// (ViewMatrix * ProjectionMatrix).
    ///
    /// The matrix is expected to use the depth conventions of
    /// <see cref="Matrix4x4.CreatePerspectiveFieldOfView"/> and
    /// <see cref="Matrix4x4.CreateOrthographic"/>, which map the visible
    /// depth range to normalized device coordinates from 0 (near plane)
    /// to 1 (far plane).
    /// </summary>
    /// <param name="viewProjection">
    /// Combined view-projection matrix.
    /// </param>
    /// <returns>
    /// A frustum whose six planes bound the visible volume.
    /// </returns>
    public static Frustum Create(Matrix4x4 viewProjection)
    {
        // With row-vector semantics (clip = position * viewProjection),
        // each clip coordinate is a weighted sum of one matrix column plus
        // the last matrix row. Each clipping plane below is the linear
        // inequality that keeps that coordinate inside its clip range.
        var clipX = new Vector3(
            viewProjection.M11,
            viewProjection.M21,
            viewProjection.M31);

        var clipY = new Vector3(
            viewProjection.M12,
            viewProjection.M22,
            viewProjection.M32);

        var clipZ = new Vector3(
            viewProjection.M13,
            viewProjection.M23,
            viewProjection.M33);

        var clipW = new Vector3(
            viewProjection.M14,
            viewProjection.M24,
            viewProjection.M34);

        return new Frustum(
            near: CreatePlane(
                clipZ,
                viewProjection.M43),
            far: CreatePlane(
                clipW - clipZ,
                viewProjection.M44 - viewProjection.M43),
            left: CreatePlane(
                clipW + clipX,
                viewProjection.M44 + viewProjection.M41),
            right: CreatePlane(
                clipW - clipX,
                viewProjection.M44 - viewProjection.M41),
            top: CreatePlane(
                clipW - clipY,
                viewProjection.M44 - viewProjection.M42),
            bottom: CreatePlane(
                clipW + clipY,
                viewProjection.M44 + viewProjection.M42));
    }

    /// <summary>
    /// Determines whether the specified point is inside the frustum.
    /// </summary>
    /// <param name="point">
    /// Point to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if the point is inside the frustum; otherwise <c>false</c>.
    /// </returns>
    public bool Contains(Vector3 point)
    {
        return IsInside(Near, point) &&
               IsInside(Far, point) &&
               IsInside(Left, point) &&
               IsInside(Right, point) &&
               IsInside(Top, point) &&
               IsInside(Bottom, point);
    }

    /// <summary>
    /// Builds a plane from the coefficients of the inequality
    /// <c>normal dot position + distance &gt;= 0</c>, which is satisfied by
    /// positions inside the frustum half-space.
    /// </summary>
    private static Plane CreatePlane(
        Vector3 normal,
        float distance)
    {
        var length = normal.Length();

        return new Plane(
            normal,
            distance / length);
    }

    private static bool IsInside(
        Plane plane,
        Vector3 point)
    {
        return plane.GetDistanceToPoint(point) >= 0.0f;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Near: {Near}, Far: {Far}, Left: {Left}, Right: {Right}, Top: {Top}, Bottom: {Bottom}";
    }
}