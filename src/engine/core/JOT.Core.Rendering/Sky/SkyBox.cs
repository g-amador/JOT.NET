using JOT.Core.Rendering.Meshes;

namespace JOT.Core.Rendering.Sky;

/// <summary>
/// Represents a camera-centered sky cube textured through a cube map.
///
/// The geometry is a full cube centered on the origin; the camera
/// positions it every frame and shader code samples the cube map by
/// the local vertex position, which then acts as the view direction.
/// </summary>
public sealed class SkyBox
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SkyBox"/> class.
    /// </summary>
    /// <param name="size">Cube edge length in world units.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the size is not positive.
    /// </exception>
    public SkyBox(float size)
    {
        if (size <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size),
                size,
                "Sky box size must be positive.");
        }

        Geometry = new Mesh(
            "SkyBox",
            CreateVertices(size / 2.0f));
    }

    /// <summary>
    /// Gets the sky cube mesh.
    /// </summary>
    public Mesh Geometry { get; }

    private static float[] CreateVertices(
        float half)
    {
        const int FaceCount = 6;

        const int VerticesPerFace = 6;

        // One triangle pair per face, ordered as cube map faces bind on
        // upload: positive X, negative X, positive Y, negative Y,
        // positive Z, negative Z.

        var faces = new[]
        {
            new[] { -half, -half,  half,  half, -half,  half,  half,  half,  half, -half, half,  half },
            new[] {  half, -half, -half, -half, -half, -half, -half,  half, -half,  half, half, -half },
            new[] {  half, -half,  half,  half, -half, -half,  half,  half, -half,  half, half,  half },
            new[] { -half, -half, -half, -half, -half,  half, -half,  half,  half, -half, half, -half },
            new[] { -half,  half,  half,  half,  half,  half,  half,  half, -half, -half, half, -half },
            new[] { -half, -half, -half,  half, -half, -half,  half, -half,  half, -half, -half,  half },
        };

        Span<int> cornerPattern =
        [
            0, 1, 2,
            0, 2, 3,
        ];

        var vertices = new float[FaceCount * VerticesPerFace * Mesh.ComponentsPerVertex];

        var current = 0;

        for (var face = 0; face < FaceCount; ++face)
        {
            var corners = faces[face];

            for (var i = 0; i < VerticesPerFace; ++i)
            {
                var corner = cornerPattern[i] * 3;

                vertices[current++] = corners[corner + 0];
                vertices[current++] = corners[corner + 1];
                vertices[current++] = corners[corner + 2];
                vertices[current++] = 1.0f;
                vertices[current++] = 1.0f;
                vertices[current++] = 1.0f;
                // The sky box is not lit; the normal is a placeholder
                // required by the fixed vertex layout.
                vertices[current++] = 0.0f;
                vertices[current++] = 1.0f;
                vertices[current++] = 0.0f;
                vertices[current++] = 0.0f;
                vertices[current++] = 0.0f;
            }
        }

        return vertices;
    }
}
