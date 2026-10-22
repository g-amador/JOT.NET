using System.Numerics;
using JOT.Core.Rendering.Meshes;

namespace JOT.Core.Rendering.Sky;

/// <summary>
/// Represents a camera-centered panoramic sky dome.
///
/// The geometry is an upper hemisphere extending below the horizon so
/// it closes against the ground plane; the camera positions it every
/// frame and shader code samples the panorama by the
/// latitude-longitude UV layout.
/// </summary>
public sealed class SkyDome
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SkyDome"/> class.
    /// </summary>
    /// <param name="radius">Dome radius in world units.</param>
    /// <param name="slices">Number of segments around the dome.</param>
    /// <param name="stacks">Number of segments from zenith to horizon.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the radius is not positive or the segment counts are
    /// outside their supported ranges.
    /// </exception>
    public SkyDome(
        float radius,
        int slices = 32,
        int stacks = 8)
    {
        if (radius <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                radius,
                "Sky dome radius must be positive.");
        }

        if (slices < 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(slices),
                slices,
                "Sky dome slices must be at least three.");
        }

        if (stacks < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stacks),
                stacks,
                "Sky dome stacks must be at least one.");
        }

        Geometry = new Mesh(
            "SkyDome",
            CreateVertices(radius, slices, stacks));
    }

    /// <summary>
    /// Gets the sky dome mesh.
    /// </summary>
    public Mesh Geometry { get; }

    private static float[] CreateVertices(
        float radius,
        int slices,
        int stacks)
    {
        // Extends below the horizon so the dome closes against the
        // ground plane; 20% covers the far edge of the 20-unit ground
        // plane from the smoke camera's eye height.

        var maxPolarAngle = MathF.PI / 2.0f * 1.2f;

        var vertices = new float[slices * stacks * 6 * Mesh.ComponentsPerVertex];

        var current = 0;

        for (var stack = 0; stack < stacks; ++stack)
        {
            var theta0 = maxPolarAngle * stack / stacks;
            var theta1 = maxPolarAngle * (stack + 1) / stacks;

            for (var slice = 0; slice < slices; ++slice)
            {
                var phi0 = 2.0f * MathF.PI * slice / slices;
                var phi1 = 2.0f * MathF.PI * (slice + 1) / slices;

                var a = ComputeVertex(radius, theta0, phi0, slices, maxPolarAngle);
                var b = ComputeVertex(radius, theta1, phi0, slices, maxPolarAngle);
                var c = ComputeVertex(radius, theta1, phi1, slices, maxPolarAngle);
                var d = ComputeVertex(radius, theta0, phi1, slices, maxPolarAngle);

                current = WriteTriangle(
                    vertices,
                    current,
                    a,
                    b,
                    c);

                current = WriteTriangle(
                    vertices,
                    current,
                    a,
                    c,
                    d);
            }
        }

        return vertices;
    }

    private static (float X, float Y, float Z, float U, float V) ComputeVertex(
        float radius,
        float theta,
        float phi,
        int slices,
        float maxPolarAngle)
    {
        return (
            radius * MathF.Sin(theta) * MathF.Cos(phi),
            radius * MathF.Cos(theta),
            radius * MathF.Sin(theta) * MathF.Sin(phi),
            phi / (2.0f * MathF.PI),
            theta / maxPolarAngle);
    }

    private static int WriteTriangle(
        float[] vertices,
        int current,
        (float X, float Y, float Z, float U, float V) a,
        (float X, float Y, float Z, float U, float V) b,
        (float X, float Y, float Z, float U, float V) c)
    {
        current = WriteVertex(
            vertices,
            current,
            a);

        current = WriteVertex(
            vertices,
            current,
            b);

        return WriteVertex(
            vertices,
            current,
            c);
    }

    private static int WriteVertex(
        float[] vertices,
        int current,
        (float X, float Y, float Z, float U, float V) vertex)
    {
        vertices[current++] = vertex.X;
        vertices[current++] = vertex.Y;
        vertices[current++] = vertex.Z;
        vertices[current++] = 1.0f;
        vertices[current++] = 1.0f;
        vertices[current++] = 1.0f;
        // The sky dome is not lit; the normal is a placeholder required
        // by the fixed vertex layout.
        vertices[current++] = 0.0f;
        vertices[current++] = 1.0f;
        vertices[current++] = 0.0f;
        vertices[current++] = vertex.U;
        vertices[current++] = vertex.V;

        return current;
    }
}
