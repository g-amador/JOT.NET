// Disposable validation harness for Phase 2, Step 2.
// Proves the Core renderer pipeline end to end on Infrastructure glue:
// window -> IGraphicsContext -> SimpleRenderer -> Mesh -> triangles.
// Draws one camera-projected triangle for about two seconds, or until
// Escape is pressed, then exits automatically.

using JOT.Core.Rendering.Cameras;
using JOT.Core.Rendering.Meshes;
using JOT.Core.Rendering.Renderers;
using JOT.Infrastructure.Silk.Graphics;
using JOT.Infrastructure.Silk.Input;
using JOT.Infrastructure.Silk.Windowing;
using System.Numerics;

const string VertexShaderSource = """
    #version 330 core

    layout (location = 0) in vec3 position;
    layout (location = 1) in vec3 color;

    uniform mat4 transform;

    out vec3 vertexColor;

    void main()
    {
        vertexColor = color;

        gl_Position = transform * vec4(position, 1.0);
    }
    """;

const string FragmentShaderSource = """
    #version 330 core

    in vec3 vertexColor;

    out vec4 fragmentColor;

    void main()
    {
        fragmentColor = vec4(vertexColor, 1.0);
    }
    """;

float[] TriangleVertices =
[
    //   positions            colors
    -0.5f, -0.5f, 0.0f,   1.0f, 0.0f, 0.0f,
     0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,
     0.0f,  0.5f, 0.0f,   0.0f, 0.0f, 1.0f,
];

var window = new SilkWindow("JOT.NET Core Rendering Smoke", 1280, 720);

var camera = new PerspectiveCamera
{
    AspectRatio = 1280.0f / 720.0f,
};

camera.Transform.Position = new Vector3(0.0f, 0.0f, 2.0f);

var viewProjection = camera.ViewMatrix * camera.ProjectionMatrix;

SimpleRenderer? renderer = null;

IShaderProgram? shaderProgram = null;

IVertexBuffer? triangle = null;

var frameCount = 0;

window.Run((currentWindow, deltaTime) =>
{
    if (renderer is null)
    {
        renderer = new SimpleRenderer(currentWindow.Graphics);

        shaderProgram = renderer.CreateShaderProgram(
            VertexShaderSource,
            FragmentShaderSource);

        triangle = renderer.CreateVertexBuffer(
            new Mesh("Triangle", TriangleVertices));
    }

    renderer.Clear(0.1f, 0.15f, 0.35f, 1.0f);

    renderer.DrawTriangles(
        triangle!,
        shaderProgram!,
        "transform",
        viewProjection);

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Escape)
        || ++frameCount >= 120)
    {
        currentWindow.Close();
    }
});

Console.WriteLine($"Smoke harness closed normally after {frameCount} frames.");
