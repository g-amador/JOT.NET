using Assimp;
using System.Numerics;

namespace JOT.Infrastructure.Assimp.Importing;

/// <summary>
/// AssimpNet implementation of a model importer.
///
/// Imports are triangulated, UV coordinates are flipped into the
/// OpenGL convention, smooth normals are generated when missing, and
/// identical vertices are welded together.
/// </summary>
public sealed class AssimpModelImporter : IModelImporter
{
    private readonly AssimpContext _context = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        _context.Dispose();
    }

    /// <inheritdoc/>
    public ImportedModel Import(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Model file not found: '{filePath}'.",
                filePath);
        }

        var scene = _context.ImportFile(
            filePath,
            PostProcessSteps.Triangulate
            | PostProcessSteps.GenerateSmoothNormals
            | PostProcessSteps.JoinIdenticalVertices
            | PostProcessSteps.FlipUVs);

        if (scene is null)
        {
            throw new InvalidOperationException(
                $"Failed to import model file: '{filePath}'.");
        }

        var materials = new List<ImportedMaterial>(scene.MaterialCount);

        foreach (var material in scene.Materials)
        {
            materials.Add(new ImportedMaterial
            {
                Name = material.Name,
                DiffuseColor = new Vector3(
                    material.ColorDiffuse.R,
                    material.ColorDiffuse.G,
                    material.ColorDiffuse.B),
                DiffuseTexturePath = material.HasTextureDiffuse
                    ? material.TextureDiffuse.FilePath
                    : null,
            });
        }

        var meshes = new List<ImportedMesh>(scene.MeshCount);

        foreach (var mesh in scene.Meshes)
        {
            meshes.Add(ImportMesh(mesh));
        }

        return new ImportedModel
        {
            Name = Path.GetFileName(filePath),
            Meshes = meshes,
            Materials = materials,
        };
    }

    private static ImportedMesh ImportMesh(
        Mesh mesh)
    {
        var vertexCount = mesh.VertexCount;

        var positions = new float[vertexCount * 3];

        for (var i = 0; i < vertexCount; ++i)
        {
            var vertex = mesh.Vertices[i];

            positions[i * 3 + 0] = vertex.X;
            positions[i * 3 + 1] = vertex.Y;
            positions[i * 3 + 2] = vertex.Z;
        }

        var normals = new float[vertexCount * 3];

        if (mesh.HasNormals)
        {
            for (var i = 0; i < vertexCount; ++i)
            {
                var normal = mesh.Normals[i];

                normals[i * 3 + 0] = normal.X;
                normals[i * 3 + 1] = normal.Y;
                normals[i * 3 + 2] = normal.Z;
            }
        }

        var uvCoordinates = new float[vertexCount * 2];

        if (mesh.HasTextureCoords(0))
        {
            var channel = mesh.TextureCoordinateChannels[0];

            for (var i = 0; i < vertexCount; ++i)
            {
                uvCoordinates[i * 2 + 0] = channel[i].X;
                uvCoordinates[i * 2 + 1] = channel[i].Y;
            }
        }

        return new ImportedMesh
        {
            Name = mesh.Name,
            Positions = positions,
            Normals = normals,
            UvCoordinates = uvCoordinates,
            Indices = GetTriangleIndices(mesh),
            MaterialIndex = mesh.MaterialIndex,
        };
    }

    private static uint[] GetTriangleIndices(
        Mesh mesh)
    {
        var sourceIndices = mesh.GetIndices();

        var indices = new uint[sourceIndices.Length];

        for (var i = 0; i < sourceIndices.Length; ++i)
        {
            indices[i] = (uint)sourceIndices[i];
        }

        return indices;
    }
}
