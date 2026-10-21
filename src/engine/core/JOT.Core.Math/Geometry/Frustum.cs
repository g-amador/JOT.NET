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

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Near: {Near}, Far: {Far}, Left: {Left}, Right: {Right}, Top: {Top}, Bottom: {Bottom}";
    }
}