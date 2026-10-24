using Assimp;
using StbImageSharp;
using System.Numerics;
using AssimpMatrix = Assimp.Matrix4x4;

namespace JOT.Infrastructure.Assimp.Importing;

/// <summary>
/// AssimpNet implementation of a model importer.
///
/// Imports are triangulated, UV coordinates are flipped into the
/// OpenGL convention, smooth normals are generated when missing, and
/// identical vertices are welded together.
///
/// Only Wavefront OBJ and glTF 2 (glTF/GLB) files are accepted; all
/// other formats fail loudly. The engine supports exactly these two
/// authoring formats.
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

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        if (extension is not (".obj" or ".gltf" or ".glb"))
        {
            throw new NotSupportedException(
                $"Unsupported model format '{extension}' in '{filePath}'. " +
                "Only Wavefront OBJ (.obj) and glTF 2 (.gltf/.glb) files are supported.");
        }

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

        // Bake the transform chain of every node into its referenced
        // mesh: models with hierarchy offsets (glTF binary usually) then
        // import in their final world-space shape.

        var meshTransforms = new System.Numerics.Matrix4x4[scene.MeshCount];

        CollectNodeTransforms(
            scene.RootNode,
            System.Numerics.Matrix4x4.Identity,
            meshTransforms);

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
                DiffuseEmbeddedTexture = LoadEmbeddedDiffuse(scene, material),
            });
        }

        var meshes = new List<ImportedMesh>(scene.MeshCount);

        for (var i = 0; i < scene.MeshCount; ++i)
        {
            meshes.Add(ImportMesh(scene.Meshes[i], meshTransforms[i]));
        }

        return new ImportedModel
        {
            Name = Path.GetFileName(filePath),
            Meshes = meshes,
            Materials = materials,
        };
    }

    private static void CollectNodeTransforms(
        Node node,
        System.Numerics.Matrix4x4 parent,
        System.Numerics.Matrix4x4[] meshTransforms)
    {
        // Assimp composes node chains as parent * child with row-vectors;
        // converting each matrix on the System.Numerics column-vector
        // convention reproduces the same world shapes.

        var current = parent * ToNumerics(node.Transform);

        foreach (var meshIndex in node.MeshIndices)
        {
            if (meshIndex >= 0 && meshIndex < meshTransforms.Length)
            {
                meshTransforms[meshIndex] = current;
            }
        }

        foreach (var child in node.Children)
        {
            CollectNodeTransforms(child, current, meshTransforms);
        }
    }

    private static System.Numerics.Matrix4x4 ToNumerics(
        AssimpMatrix matrix)
    {
        // Assimp matrices are stored for row-vector math with the
        // translation in the last column (a4, b4, c4); System.Numerics
        // multiplies column vectors, so the linear map is the transpose
        // and the translation row lands in the System translation
        // positions (M41..M43).

        return new System.Numerics.Matrix4x4(
            matrix.A1, matrix.B1, matrix.C1, matrix.D1,
            matrix.A2, matrix.B2, matrix.C2, matrix.D2,
            matrix.A3, matrix.B3, matrix.C3, matrix.D3,
            matrix.A4, matrix.B4, matrix.C4, matrix.D4);
    }

    private static RawImage? LoadEmbeddedDiffuse(
        Scene scene,
        Material material)
    {
        if (!material.HasTextureDiffuse
            || material.TextureDiffuse.FilePath.Length < 2
            || material.TextureDiffuse.FilePath[0] != '*')
        {
            return null;
        }

        if (!int.TryParse(
                material.TextureDiffuse.FilePath[1..],
                out var textureIndex)
            || textureIndex < 0
            || textureIndex >= scene.TextureCount)
        {
            return null;
        }

        var embedded = scene.Textures[textureIndex];

        if (embedded.IsCompressed)
        {
            // The embed holds a compressed PNG/JPEG byte stream; decode
            // it the same way files on disk are decoded.

            var decoded = ImageResult.FromMemory(
                embedded.CompressedData,
                ColorComponents.RedGreenBlueAlpha);

            return new RawImage
            {
                SourcePath = $"*{textureIndex}",
                Width = decoded.Width,
                Height = decoded.Height,
                Pixels = decoded.Data,
            };
        }

        // The embed holds Width x Height uncompressed ARGB8888 texels;
        // convert into the RGBA contract of RawImage.

        var texelCount = embedded.NonCompressedData.Length;

        var pixels = new byte[texelCount * 4];

        for (var i = 0; i < texelCount; ++i)
        {
            var texel = embedded.NonCompressedData[i];

            var offset = i * 4;

            pixels[offset + 0] = texel.R;
            pixels[offset + 1] = texel.G;
            pixels[offset + 2] = texel.B;
            pixels[offset + 3] = texel.A;
        }

        return new RawImage
        {
            SourcePath = $"*{textureIndex}",
            Width = embedded.Width,
            Height = embedded.Height,
            Pixels = pixels,
        };
    }

    private static ImportedMesh ImportMesh(
        Mesh mesh,
        System.Numerics.Matrix4x4 world)
    {
        var vertexCount = mesh.VertexCount;

        var positions = new float[vertexCount * 3];

        for (var i = 0; i < vertexCount; ++i)
        {
            var worldVertex = System.Numerics.Vector3.Transform(
                new System.Numerics.Vector3(
                    mesh.Vertices[i].X,
                    mesh.Vertices[i].Y,
                    mesh.Vertices[i].Z),
                world);

            positions[i * 3 + 0] = worldVertex.X;
            positions[i * 3 + 1] = worldVertex.Y;
            positions[i * 3 + 2] = worldVertex.Z;
        }

        var normals = new float[vertexCount * 3];

        if (mesh.HasNormals)
        {
            for (var i = 0; i < vertexCount; ++i)
            {
                // The linear map only (rotation and scale); renormalize
                // so scaled hierarchies keep unit-length normals.

                var worldNormal = System.Numerics.Vector3.Normalize(
                    System.Numerics.Vector3.TransformNormal(
                        new System.Numerics.Vector3(
                            mesh.Normals[i].X,
                            mesh.Normals[i].Y,
                            mesh.Normals[i].Z),
                        world));

                normals[i * 3 + 0] = worldNormal.X;
                normals[i * 3 + 1] = worldNormal.Y;
                normals[i * 3 + 2] = worldNormal.Z;
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
