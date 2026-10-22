// The JOT.NET Core Template validation program (AI.md success criteria);
// currently at Step 2: textured models loaded from assets/models (OBJ
// and glTF 2 only), baked into the fixed vertex layout and placed on
// the procedural textured ground plane through the Infrastructure glue
// (SilkWindow, image loader, ITexture2D, Assimp importer, SimpleRenderer
// shader programs). The vertex layout is fixed as position (3), color
// (3), normal (3), uv (2).

using System.Numerics;
using JOT.Core.Math.Transformations;
using JOT.Core.Rendering.Cameras;
using JOT.Core.Rendering.Meshes;
using JOT.Core.Rendering.Renderers;
using JOT.Infrastructure.Assimp.Importing;
using JOT.Infrastructure.Silk.Graphics;
using JOT.Infrastructure.Silk.Input;
using JOT.Infrastructure.Silk.Windowing;

// The textured scene uses the fixed interleaved vertex layout from
// Mesh: position (3), color (3), normal (3), uv (2).

const string VertexShaderSource = """
    #version 330 core

    layout (location = 0) in vec3 position;
    layout (location = 1) in vec3 color;
    layout (location = 2) in vec3 normal;
    layout (location = 3) in vec2 uv;

    uniform mat4 uViewProjection;
    uniform mat4 uModel;

    out vec3 vertexColor;
    out vec2 vertexUv;

    void main()
    {
        vertexColor = color;
        vertexUv = uv;

        gl_Position = uViewProjection * (uModel * vec4(position, 1.0));
    }
    """;

const string FragmentShaderSource = """
    #version 330 core

    in vec3 vertexColor;
    in vec2 vertexUv;

    out vec4 fragmentColor;

    uniform sampler2D uTexture;

    void main()
    {
        fragmentColor = texture(uTexture, vertexUv) * vec4(vertexColor, 1.0);
    }
    """;

// Procedural ground plane centered on the origin, 8x8 texture repeats.

float[] CreateGroundVertices(
    float halfSize)
{
    const int stride = Mesh.ComponentsPerVertex;

    var vertices = new float[6 * stride];

    float[,] corners =
    {
        { -halfSize, -halfSize },
        { halfSize, -halfSize },
        { halfSize, halfSize },
        { -halfSize, halfSize },
    };

    // Two triangles, corners ordered counter-clockwise when viewed
    // from above (+Y), with rows: position, color, normal, uv.

    int[] indices =
    {
        0, 1, 2,
        0, 2, 3,
    };

    for (var i = 0; i < indices.Length; ++i)
    {
        var corner = indices[i];

        vertices[i * stride + 0] = corners[corner, 0];
        vertices[i * stride + 1] = 0.0f;
        vertices[i * stride + 2] = corners[corner, 1];

        vertices[i * stride + 3] = 1.0f;
        vertices[i * stride + 4] = 1.0f;
        vertices[i * stride + 5] = 1.0f;

        vertices[i * stride + 6] = 0.0f;
        vertices[i * stride + 7] = 1.0f;
        vertices[i * stride + 8] = 0.0f;

        const float uvRange = 8.0f;

        vertices[i * stride + 9] = (corners[corner, 0] / halfSize + 1.0f) * 0.5f * uvRange;
        vertices[i * stride + 10] = (1.0f - (corners[corner, 1] / halfSize + 1.0f) * 0.5f) * uvRange;
    }

    return vertices;
}

// Bakes an imported mesh into triangle-soup vertices by expanding the
// index list. DrawArrays consumes the consecutive triples directly;
// index buffer glue arrives with a later step.

float[] BakeMeshVertices(
    ImportedMesh mesh)
{
    const int stride = Mesh.ComponentsPerVertex;

    var vertices = new float[mesh.Indices.Length * stride];

    for (var i = 0; i < mesh.Indices.Length; ++i)
    {
        var source = (int)mesh.Indices[i];

        vertices[i * stride + 0] = mesh.Positions[source * 3 + 0];
        vertices[i * stride + 1] = mesh.Positions[source * 3 + 1];
        vertices[i * stride + 2] = mesh.Positions[source * 3 + 2];

        vertices[i * stride + 3] = 1.0f;
        vertices[i * stride + 4] = 1.0f;
        vertices[i * stride + 5] = 1.0f;

        if (source * 3 + 2 < mesh.Normals.Length)
        {
            vertices[i * stride + 6] = mesh.Normals[source * 3 + 0];
            vertices[i * stride + 7] = mesh.Normals[source * 3 + 1];
            vertices[i * stride + 8] = mesh.Normals[source * 3 + 2];
        }
        else
        {
            vertices[i * stride + 6] = 0.0f;
            vertices[i * stride + 7] = 1.0f;
            vertices[i * stride + 8] = 0.0f;
        }

        vertices[i * stride + 9] = mesh.UvCoordinates[source * 2 + 0];
        vertices[i * stride + 10] = mesh.UvCoordinates[source * 2 + 1];
    }

    return vertices;
}

Transform CreateTransform(
    Vector3 position,
    Vector3 scale,
    float yaw,
    float pitch)
{
    var transform = new Transform();

    transform.Position = position;
    transform.Rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0.0f);
    transform.Scale = scale;

    return transform;
}

string FindAssetsDirectory()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null)
    {
        var assetsDirectory = Path.Combine(
            directory.FullName,
            "assets");

        if (Directory.Exists(assetsDirectory))
        {
            return assetsDirectory;
        }

        directory = directory.Parent!;
    }

    throw new DirectoryNotFoundException(
        $"Failed to locate the assets directory from '{AppContext.BaseDirectory}'.");
}

var assetsDirectory = FindAssetsDirectory();

Console.WriteLine($"Assets directory: {assetsDirectory}");

var imageLoader = new StbImageLoader();

var window = new SilkWindow("JOT.NET Core Template", 1280, 720);

var camera = new PerspectiveCamera
{
    AspectRatio = 1280.0f / 720.0f,
};

camera.Transform.Position = new Vector3(0.0f, 2.0f, 4.0f);

// Pitch the camera down toward the ground plane.

camera.Transform.Rotation = Quaternion.CreateFromAxisAngle(
    Vector3.UnitX,
    -0.35f);

var viewProjection = camera.ViewMatrix * camera.ProjectionMatrix;

// Optional frame limit for automated validation runs; without the
// argument the template runs until Escape is pressed.

int? frameLimit = null;

for (var i = 0; i < args.Length - 1; ++i)
{
    if (args[i] == "--frames")
    {
        frameLimit = int.Parse(args[i + 1]);
    }
}

SimpleRenderer? renderer = null;

IShaderProgram? shaderProgram = null;

var sceneItems = new List<SceneItem>();

var frameCount = 0;

window.Run((currentWindow, deltaTime) =>
{
    if (renderer is null)
    {
        renderer = new SimpleRenderer(currentWindow.Graphics);

        shaderProgram = renderer.CreateShaderProgram(
            VertexShaderSource,
            FragmentShaderSource);

        renderer.SetViewport(1280, 720);

        var texturesByPath = new Dictionary<string, ITexture2D>();

        ITexture2D CreateTextureFromFile(
            string texturePath)
        {
            if (texturesByPath.TryGetValue(texturePath, out var existing))
            {
                return existing;
            }

            var image = imageLoader.Load(texturePath);

            var texture = renderer.CreateTexture(
                image.Width,
                image.Height,
                image.Pixels);

            texturesByPath.Add(texturePath, texture);

            Console.WriteLine($"Loaded texture '{Path.GetFileName(texturePath)}' ({image.Width}x{image.Height}).");

            return texture;
        }

        // Ground plane: procedural geometry, grass material.

        sceneItems.Add(
            new SceneItem(
                renderer.CreateVertexBuffer(
                    new Mesh("Ground", CreateGroundVertices(10.0f))),
                CreateTextureFromFile(
                    Path.Combine(
                        assetsDirectory,
                        "textures",
                        "materials",
                        "greengrass",
                        "greengrass-lambertian.png")),
                new Transform()));

        // Model instances: placeholder assets under assets/models; the
        // material textures come from assets/textures/materials. Every
        // model file fails loudly when missing or of a wrong format.

        var modelsDirectory = Path.Combine(assetsDirectory, "models");

        var materialTexturesDirectory = Path.Combine(
            assetsDirectory,
            "textures",
            "materials");

        using var modelImporter = new AssimpModelImporter();

        var modelsByFile = new Dictionary<string, ImportedModel>();

        ImportedModel ImportModel(
            string modelFile)
        {
            if (modelsByFile.TryGetValue(modelFile, out var existing))
            {
                return existing;
            }

            var model = modelImporter.Import(
                Path.Combine(modelsDirectory, modelFile));

            modelsByFile.Add(modelFile, model);

            var totalTriangles = 0;

            foreach (var mesh in model.Meshes)
            {
                totalTriangles += mesh.Indices.Length / 3;
            }

            Console.WriteLine(
                $"Loaded model '{model.Name}': {model.Meshes.Count} mesh(es), {totalTriangles} triangles.");

            return model;
        }

        var sceneInstances = new (string ModelFile, string TextureDirectory, string TextureFile, Transform Transform)[]
        {
            ("cow.obj", "rockydirt", "rockydirt-lambertian.png",
                CreateTransform(new Vector3(-3.0f, 0.0f, -1.0f), Vector3.One, -1.2f, 0.0f)),
            ("brick_cube.obj", "redbrick", "redbrick-lambertian.png",
                CreateTransform(new Vector3(2.2f, 0.0f, 0.2f), Vector3.One, 0.0f, 0.0f)),
            ("brick_cube.obj", "redbrick", "redbrick-lambertian.png",
                CreateTransform(new Vector3(3.2f, 0.0f, -0.6f), Vector3.One, 0.7f, 0.0f)),
            ("fighter.obj", "metalcrate", "metalcrate-lambertian.png",
                CreateTransform(new Vector3(0.0f, 1.6f, -3.5f), Vector3.One, -1.9f, 0.15f)),
            ("cone.obj", "stucco", "stucco-lambertian.png",
                CreateTransform(new Vector3(4.5f, 0.0f, -3.0f), new Vector3(2.2f, 2.8f, 2.2f), 0.0f, 0.0f)),
        };

        foreach (var instance in sceneInstances)
        {
            var model = ImportModel(instance.ModelFile);

            var texture = CreateTextureFromFile(
                Path.Combine(
                    materialTexturesDirectory,
                    instance.TextureDirectory,
                    instance.TextureFile));

            foreach (var mesh in model.Meshes)
            {
                sceneItems.Add(
                    new SceneItem(
                        renderer.CreateVertexBuffer(
                            new Mesh(
                                mesh.Name,
                                BakeMeshVertices(mesh))),
                        texture,
                        instance.Transform));
            }
        }
    }

    renderer!.Clear(0.1f, 0.15f, 0.35f, 1.0f);

    foreach (var item in sceneItems)
    {
        renderer.DrawTriangles(
            item.Buffer,
            shaderProgram!,
            "uViewProjection",
            viewProjection,
            "uModel",
            item.Transform.WorldMatrix,
            item.Texture,
            "uTexture");
    }

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Escape)
        || (frameLimit is int limit && ++frameCount >= limit))
    {
        currentWindow.Close();

        Console.WriteLine($"Template closed normally after {frameCount} frames.");
    }
});

// Identifies one textured, positioned entry of the scene composition.

record SceneItem(
    IVertexBuffer Buffer,
    ITexture2D Texture,
    Transform Transform);
