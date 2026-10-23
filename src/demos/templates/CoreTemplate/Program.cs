// The JOT.NET Core Template validation program (AI.md success criteria);
// currently at Step 7: multiple camera views — the C key cycles the
// orbit view, a grass-level cow-side view and a near-top-down view;
// the view-projection, sky containers and audio listener all follow
// the active camera.

using System.Numerics;
using JOT.Core.Audio;
using JOT.Core.Math.Geometry;
using JOT.Core.Math.Transformations;
using JOT.Core.Rendering.Cameras;
using JOT.Core.Rendering.Lights;
using JOT.Core.Rendering.Meshes;
using JOT.Core.Rendering.Renderers;
using JOT.Core.Rendering.Sky;
using JOT.Infrastructure.Assimp.Importing;
using JOT.Infrastructure.Silk.Audio;
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
    uniform mat4 uLightViewProj;

    out vec3 vertexColor;
    out vec2 vertexUv;
    out vec3 vertexNormal;
    out vec4 vertexShadowPosition;

    void main()
    {
        vertexColor = color;
        vertexUv = uv;
        vertexNormal = normal;

        vec4 worldPosition = uModel * vec4(position, 1.0);

        vertexShadowPosition = uLightViewProj * worldPosition;

        gl_Position = uViewProjection * worldPosition;
    }
    """;

const string FragmentShaderSource = """
    #version 330 core

    in vec3 vertexColor;
    in vec2 vertexUv;
    in vec3 vertexNormal;
    in vec4 vertexShadowPosition;

    out vec4 fragmentColor;

    uniform sampler2D uTexture;
    uniform sampler2D uShadowMap;
    uniform vec3 uLightDirection;
    uniform vec3 uLightColor;
    uniform float uAmbient;

    void main()
    {
        float diffuse = max(dot(normalize(vertexNormal), uLightDirection), 0.0);

        vec3 shadowCoords = vertexShadowPosition.xyz / vertexShadowPosition.w;

        shadowCoords = shadowCoords * 0.5 + 0.5;

        float shadowFactor = 1.0;

        if (shadowCoords.x > 0.0 && shadowCoords.x < 1.0 &&
            shadowCoords.y > 0.0 && shadowCoords.y < 1.0)
        {
            float occluderDepth = texture(uShadowMap, shadowCoords.xy).r;

            if (occluderDepth < shadowCoords.z - 0.003)
            {
                shadowFactor = 0.0;
            }
        }

        vec3 light = uLightColor * (uAmbient + diffuse * shadowFactor);

        fragmentColor = texture(uTexture, vertexUv) * vec4(vertexColor, 1.0) * vec4(light, 1.0);
    }
    """;

const string ShadowVertexShaderSource = """
    #version 330 core

    layout (location = 0) in vec3 position;

    uniform mat4 transform;

    void main()
    {
        gl_Position = transform * vec4(position, 1.0);
    }
    """;

const string ShadowFragmentShaderSource = """
    #version 330 core

    out vec4 fragmentColor;

    void main()
    {
        fragmentColor = vec4(1.0);
    }
    """;

// The container program draws white wireframe containers: position and
// color only, spanning the fixed vertex layout.

const string ContainerVertexShaderSource = """
    #version 330 core

    layout (location = 0) in vec3 position;
    layout (location = 1) in vec3 color;

    uniform mat4 uViewProjection;
    uniform mat4 uModel;

    out vec3 vertexColor;

    void main()
    {
        vertexColor = color;

        gl_Position = uViewProjection * (uModel * vec4(position, 1.0));
    }
    """;

const string ContainerFragmentShaderSource = """
    #version 330 core

    in vec3 vertexColor;

    out vec4 fragmentColor;

    void main()
    {
        fragmentColor = vec4(vertexColor, 1.0);
    }
    """;

// The sky box draws through its own program: the vertex position is
// the local view direction, sampled by the cube map.

const string SkyBoxVertexShaderSource = """
    #version 330 core

    layout (location = 0) in vec3 position;

    uniform mat4 transform;

    out vec3 skyDirection;

    void main()
    {
        skyDirection = position;

        gl_Position = transform * vec4(position, 1.0);
    }
    """;

const string SkyBoxFragmentShaderSource = """
    #version 330 core

    in vec3 skyDirection;

    out vec4 fragmentColor;

    uniform samplerCube uSky;

    void main()
    {
        fragmentColor = texture(uSky, normalize(skyDirection));
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

// Builds the white wireframe container of one model instance: the local
// bounds of the baked triangles go through the JOT.Core.Math AABB,
// transformed by the instance world matrix, then become the 12 box
// edges as line-pair vertices in the shared layout.

float[] CreateContainerVertices(
    Vector3 localMin,
    Vector3 localMax,
    Matrix4x4 world)
{
    var box = new AxisAlignedBoundingBox(localMin, localMax)
        .Transform(world);

    var corners = new Vector3[8];

    for (var i = 0; i < 8; ++i)
    {
        corners[i] = new Vector3(
            (i & 1) == 0 ? box.Min.X : box.Max.X,
            (i & 2) == 0 ? box.Min.Y : box.Max.Y,
            (i & 4) == 0 ? box.Min.Z : box.Max.Z);
    }

    // The 12 edges as corner index pairs.

    Span<int> edges =
    [
        0, 1, 1, 3, 3, 2, 2, 0, // bottom face
        4, 5, 5, 7, 7, 6, 6, 4, // top face
        0, 4, 1, 5, 2, 6, 3, 7, // pillars
    ];

    var vertices = new float[edges.Length * Mesh.ComponentsPerVertex];

    for (var i = 0; i < edges.Length; ++i)
    {
        var corner = corners[edges[i]];

        var vertex = i * Mesh.ComponentsPerVertex;

        vertices[vertex + 0] = corner.X;
        vertices[vertex + 1] = corner.Y;
        vertices[vertex + 2] = corner.Z;

        // White containers; normal and uv are placeholders required by
        // the shared vertex layout.

        vertices[vertex + 3] = 1.0f;
        vertices[vertex + 4] = 1.0f;
        vertices[vertex + 5] = 1.0f;
        vertices[vertex + 6] = 0.0f;
        vertices[vertex + 7] = 1.0f;
        vertices[vertex + 8] = 0.0f;
        vertices[vertex + 9] = 0.0f;
        vertices[vertex + 10] = 0.0f;
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

// Multiple camera views cycle on the C key: the orbit view over the
// scene, a grass-level cow-side view and a near-top-down view. Each
// aims through the Transform forward axis (identity looks down -Z;
// yaw turns around Y, pitch up is positive), computed from the view
// direction toward the looked-at point.

var cameraViews = new[] { "orbit", "cow-side", "top-down" };

var cameras = new List<PerspectiveCamera>();

PerspectiveCamera CreateCamera(
    Vector3 position,
    float yaw,
    float pitch)
{
    var camera = new PerspectiveCamera
    {
        AspectRatio = 1280.0f / 720.0f,
    };

    camera.Transform.Position = position;
    camera.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0.0f);

    cameras.Add(camera);

    return camera;
}

// Orbit view over the scene (the original full view).

_ = CreateCamera(
    new Vector3(0.0f, 2.0f, 4.0f),
    0.0f,
    -0.35f);

// Cow-side view: grass level looking across at the cow.

_ = CreateCamera(
    new Vector3(-6.0f, 1.3f, 1.5f),
    -0.876f,
    -0.32f);

// Near-top-down view over the whole ground plane.

_ = CreateCamera(
    new Vector3(0.0f, 14.0f, 0.0f),
    0.0f,
    -1.45f);

var activeCameraIndex = 0;

// Sun-like directional light over the scene. The shadow volume covers
// the whole ground plane and every model instance.

var directionalLight = new DirectionalLight
{
    Direction = Vector3.Normalize(new Vector3(0.5f, -1.0f, 0.35f)),
    Color = new Vector3(1.0f, 0.96f, 0.92f),
    AmbientStrength = 0.35f,
};

var lightViewProjection = directionalLight.CreateShadowViewProjection(
    extent: 20.0f,
    distance: 40.0f,
    nearPlane: 1.0f,
    farPlane: 100.0f);

// The sky containers and the composite view matrices follow the active
// camera; they are rebuilt every frame by the render loop.

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

IShaderProgram? shadowShaderProgram = null;

IShaderProgram? skyBoxShaderProgram = null;

IShadowRenderTarget? shadowRenderTarget = null;

IShaderProgram? containerShaderProgram = null;

ITexture2D? modelsTextureOff = null;

SilkAudioDevice? audioDevice = null;

Listener? listener = null;

SoundEmitter? coneSound = null;

ITextureCubeMap? skyBoxTexture = null;

ITexture2D? skyDomeTexture = null;

IVertexBuffer? skyBoxBuffer = null;

IVertexBuffer? skyDomeBuffer = null;

var skyMode = SkyMode.Dome;

var sceneItems = new List<SceneItem>();

var containerItems = new List<SceneItem>();

var showTextures = true;

var showContainers = false;

var wireframeTriangles = false;

var frameCount = 0;

window.Run((currentWindow, deltaTime) =>
{
    if (renderer is null)
    {
        renderer = new SimpleRenderer(currentWindow.Graphics);

        shaderProgram = renderer.CreateShaderProgram(
            VertexShaderSource,
            FragmentShaderSource);

        skyBoxShaderProgram = renderer.CreateShaderProgram(
            SkyBoxVertexShaderSource,
            SkyBoxFragmentShaderSource);

        shadowShaderProgram = renderer.CreateShaderProgram(
            ShadowVertexShaderSource,
            ShadowFragmentShaderSource);

        shadowRenderTarget = renderer.CreateShadowRenderTarget(1024);

        containerShaderProgram = renderer.CreateShaderProgram(
            ContainerVertexShaderSource,
            ContainerFragmentShaderSource);

        // White 1x1 texture for the texture-visibility toggle; drawn
        // over every model when textures are hidden.

        modelsTextureOff = renderer.CreateTexture(1, 1, [byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue]);

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

            // Walk instance triangles once for both the container
            // bounds and the debug containers list.

            var localMin = new Vector3(float.MaxValue);
            var localMax = new Vector3(float.MinValue);

            foreach (var mesh in model.Meshes)
            {
                var baked = BakeMeshVertices(mesh);

                sceneItems.Add(
                    new SceneItem(
                        renderer.CreateVertexBuffer(
                            new Mesh(
                                mesh.Name,
                                baked)),
                        texture,
                        instance.Transform));

                for (var v = 0; v < baked.Length; v += Mesh.ComponentsPerVertex)
                {
                    var position = new Vector3(
                        baked[v + 0],
                        baked[v + 1],
                        baked[v + 2]);

                    localMin = Vector3.Min(localMin, position);
                    localMax = Vector3.Max(localMax, position);
                }
            }

            containerItems.Add(
                new SceneItem(
                    renderer.CreateVertexBuffer(
                        new Mesh(
                            $"{model.Name} container",
                            CreateContainerVertices(
                                localMin,
                                localMax,
                                instance.Transform.WorldMatrix))),
                    modelsTextureOff!,
                    new Transform()));
        }

        // Sky dome: panoramic texture on the hemisphere geometry.

        skyDomeTexture = CreateTextureFromFile(
            Path.Combine(
                assetsDirectory,
                "textures",
                "skydome",
                "sky_midafternoon.jpg"));

        var skyDome = new SkyDome(
            60.0f,
            32,
            8);

        skyDomeBuffer = renderer.CreateVertexBuffer(skyDome.Geometry);

        Console.WriteLine($"Loaded sky dome texture ({skyDome.Geometry.VertexCount} dome vertices).");

        // Sky box: six-face cube map from the desert set, faces bound in
        // the OpenGL cube map order +X, -X, +Y, -Y, +Z, -Z.

        var boxDirectory = Path.Combine(
            assetsDirectory,
            "textures",
            "skybox",
            "desert");

        var boxFaceNames = new[]
        {
            "rt",
            "lf",
            "up",
            "dn",
            "ft",
            "bk",
        };

        var faces = new byte[boxFaceNames.Length][];

        var faceWidth = 0;
        var faceHeight = 0;

        for (var i = 0; i < boxFaceNames.Length; ++i)
        {
            var image = imageLoader.Load(
                Path.Combine(
                    boxDirectory,
                    $"desert_{boxFaceNames[i]}.jpg"));

            if (faceWidth == 0)
            {
                faceWidth = image.Width;
                faceHeight = image.Height;
            }
            else if (image.Width != faceWidth || image.Height != faceHeight)
            {
                throw new InvalidOperationException(
                    $"Sky box face '{boxFaceNames[i]}' is {image.Width}x{image.Height} but faces must share {faceWidth}x{faceHeight}.");
            }

            faces[i] = image.Pixels;
        }

        skyBoxTexture = renderer.CreateTextureCubeMap(
            faceWidth,
            faceHeight,
            faces);

        var skyBox = new SkyBox(100.0f);

        skyBoxBuffer = renderer.CreateVertexBuffer(skyBox.Geometry);

        Console.WriteLine($"Loaded sky box 'desert' (6 faces at {faceWidth}x{faceHeight}).");

        // Step 6: positional audio. The emitter loops the explosion boom
        // right at the cone; the listener follows the camera transform
        // each frame after the per-frame draw. The file must be
        // uncompressed 16-bit MONO PCM: OpenAL only spatializes mono
        // buffers (stereo sources ignore position).

        audioDevice = new SilkAudioDevice();

        listener = new Listener(audioDevice);

        var emitterWave = WaveFile.Load(
            Path.Combine(
                assetsDirectory,
                "sounds",
                "Effects",
                "explosion6.wav"));

        coneSound = new SoundEmitter(
            audioDevice,
            emitterWave.Samples,
            emitterWave.SampleRate,
            emitterWave.Channels)
        {
            Position = new Vector3(4.5f, 0.5f, -3.0f),
            Looping = true,
        };

        coneSound.Play();

        Console.WriteLine(
            $"Audio emitter looping 'explosion6.wav' ({emitterWave.Seconds:F1}s at {emitterWave.SampleRate} Hz, {emitterWave.Channels} ch) at the cone.");
    }

    // Cycle the sky mode on the S key.

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.S))
    {
        skyMode = skyMode switch
        {
            SkyMode.Off => SkyMode.Dome,
            SkyMode.Dome => SkyMode.Box,
            _ => SkyMode.Off,
        };

        Console.WriteLine($"Sky mode: {skyMode}");
    }

    // Cycle the active camera view on the C key.

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.C))
    {
        activeCameraIndex = (activeCameraIndex + 1) % cameras.Count;

        Console.WriteLine(
            $"Camera view: {cameraViews[activeCameraIndex]}.");
    }

    // The active camera drives the view: composite matrices and the
    // audio listener follow it every frame.

    var activeCamera = cameras[activeCameraIndex];

    var viewProjection = activeCamera.ViewMatrix * activeCamera.ProjectionMatrix;

    var skyMatrix = Matrix4x4.CreateTranslation(activeCamera.Transform.Position);

    var skyBoxComposite = skyMatrix * viewProjection;

    renderer!.Clear(0.1f, 0.15f, 0.35f, 1.0f);

    // The audio listener follows the active camera: position and
    // orientation push into OpenAL every frame so the emitter mixes
    // relative to the view.

    listener!.Transform.Position = activeCamera.Transform.Position;
    listener.Transform.Rotation = activeCamera.Transform.Rotation;
    listener.Synchronize();

    // Shadow pass: depth of the ground and the models from the light.

    shadowRenderTarget!.Begin();

    foreach (var item in sceneItems)
    {
        renderer.DrawTriangles(
            item.Buffer,
            shadowShaderProgram!,
            "transform",
            item.Transform.WorldMatrix * lightViewProjection);
    }

    shadowRenderTarget.End();

    // The shadow pass left the shadow map's viewport active; restore
    // the backbuffer viewport before drawing the scene.

    renderer.SetViewport(1280, 720);

    // Configure the lit scene program once per frame: the light
    // globals and the shadow map on unit 1. Sampler uniforms require
    // the program to be bound first.

    shaderProgram!.Use();
    shaderProgram.SetUniform("uLightViewProj", lightViewProjection);
    shaderProgram.SetUniform("uLightDirection", directionalLight.DirectionToLight);
    shaderProgram.SetUniform("uLightColor", directionalLight.Color);
    shaderProgram.SetUniform("uAmbient", directionalLight.AmbientStrength);
    shaderProgram.SetUniform("uShadowMap", 1);

    shadowRenderTarget.BindDepth(1);

    // The sky renders first at far radius; the scene draws over it.
    // The dome shares the lit scene program: outside the shadow volume
    // it is fully lit, so it stays unaffected by the shadow test.

    if (skyMode == SkyMode.Dome)
    {
        renderer.DrawTriangles(
            skyDomeBuffer!,
            shaderProgram!,
            "uViewProjection",
            viewProjection,
            "uModel",
            skyMatrix,
            skyDomeTexture!,
            "uTexture");
    }
    else if (skyMode == SkyMode.Box)
    {
        renderer.DrawTriangles(
            skyBoxBuffer!,
            skyBoxShaderProgram!,
            "transform",
            skyBoxComposite,
            skyBoxTexture!,
            "uSky");
    }

    foreach (var item in sceneItems)
    {
        renderer.DrawTriangles(
            item.Buffer,
            shaderProgram!,
            "uViewProjection",
            viewProjection,
            "uModel",
            item.Transform.WorldMatrix,
            showTextures ? item.Texture : modelsTextureOff!,
            "uTexture");
    }

    // Debug containers draw last, on top, unaffected by lighting.

    if (showContainers)
    {
        foreach (var container in containerItems)
        {
            renderer.DrawLines(
                container.Buffer,
                containerShaderProgram!,
                "uViewProjection",
                viewProjection,
                "uModel",
                container.Transform.WorldMatrix);
        }
    }

    // Debug toggle keys: 8 texture visibility, 9 wireframe triangles,
    // 0 wireframe containers.

    var keyboard = currentWindow.Input.Keyboard;

    if (keyboard.IsKeyPressed(KeyCode.Number8))
    {
        showTextures = !showTextures;

        Console.WriteLine($"Textures visible: {showTextures}.");
    }

    if (keyboard.IsKeyPressed(KeyCode.Number9))
    {
        wireframeTriangles = !wireframeTriangles;

        renderer.SetWireframe(wireframeTriangles);

        Console.WriteLine($"Wireframe triangles: {wireframeTriangles}.");
    }

    if (keyboard.IsKeyPressed(KeyCode.Number0))
    {
        showContainers = !showContainers;

        Console.WriteLine($"Containers visible: {showContainers}.");
    }

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Escape)
        || (frameLimit is int limit && ++frameCount >= limit))
    {
        Console.WriteLine($"Emitter still playing at close: {coneSound!.IsPlaying}.");

        currentWindow.Close();

        Console.WriteLine($"Template closed normally after {frameCount} frames.");
    }
});

// The OpenAL device owns a native context; release it before process
// exit instead of leaving it to the destructors.

audioDevice?.Dispose();


// Identifies one textured, positioned entry of the scene composition.

record SceneItem(
    IVertexBuffer Buffer,
    ITexture2D Texture,
    Transform Transform);
