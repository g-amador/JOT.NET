// The JOT.NET Core Template validation program (AI.md success criteria);
// currently at Step 1: a flying camera over a procedural textured ground
// plane through the Infrastructure glue (SilkWindow, image loader,
// ITexture2D, SimpleRenderer shader programs). The vertex layout is
// fixed as position (3), color (3), normal (3), uv (2).

using System.Numerics;
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

IVertexBuffer? ground = null;

ITexture2D? groundTexture = null;

var frameCount = 0;

window.Run((currentWindow, deltaTime) =>
{
    if (renderer is null)
    {
        renderer = new SimpleRenderer(currentWindow.Graphics);

        shaderProgram = renderer.CreateShaderProgram(
            VertexShaderSource,
            FragmentShaderSource);

        ground = renderer.CreateVertexBuffer(
            new Mesh("Ground", CreateGroundVertices(10.0f)));

        var groundImage = imageLoader.Load(
            Path.Combine(
                assetsDirectory,
                "textures",
                "materials",
                "greengrass",
                "greengrass-lambertian.png"));

        groundTexture = renderer.CreateTexture(
            groundImage.Width,
            groundImage.Height,
            groundImage.Pixels);

        renderer.SetViewport(1280, 720);
    }

    renderer!.Clear(0.1f, 0.15f, 0.35f, 1.0f);

    renderer.DrawTriangles(
        ground!,
        shaderProgram!,
        "uViewProjection",
        viewProjection,
        "uModel",
        Matrix4x4.Identity,
        groundTexture!,
        "uTexture");

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Escape)
        || (frameLimit is int limit && ++frameCount >= limit))
    {
        currentWindow.Close();

        Console.WriteLine($"Template closed normally after {frameCount} frames.");
    }
});
