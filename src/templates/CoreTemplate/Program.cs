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

// The HUD program draws the on-screen key options help: quads arrive
// pre-projected in clip space (the two-dimensional positions are baked
// onto the fixed vertex layout) and the fragment outputs the atlas
// color with its alpha; blending mixes it over the scene.

const string HudVertexShaderSource = """
    #version 330 core

    layout (location = 0) in vec3 position;
    layout (location = 1) in vec3 color;
    layout (location = 2) in vec3 normal;
    layout (location = 3) in vec2 uv;

    uniform mat4 uViewProjection;

    out vec2 vertexUv;

    void main()
    {
        vertexUv = uv;

        gl_Position = uViewProjection * vec4(position, 1.0);
    }
    """;

const string HudFragmentShaderSource = """
    #version 330 core

    in vec2 vertexUv;

    out vec4 fragmentColor;

    uniform sampler2D uTexture;

    void main()
    {
        vec4 texel = texture(uTexture, vertexUv);

        fragmentColor = vec4(texel.rgb, texel.a);
    }
    """;

// Generated HUD font: a 5x7 pixel bitmap per glyph, packed into
// padded 7x9 atlas cells so the linear texture filter never crosses
// into a neighboring glyph. Cell 0 holds the translucent HUD bar
// texel sampled by the bar quad; glyph cells start at index 1.
// Characters outside the atlas fail loudly, matching the asset rules.

const string HudCharset = "089ACEFIKMNORSTUWXY|";

const int HudAtlasColumns = 8;

const int HudCellWidth = 7;

const int HudCellHeight = 9;

static (char Name, string[] Rows)[] GetHudGlyphPatterns()
{
    return
    [
        ('0', [".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###."]),
        ('8', [".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###."]),
        ('9', [".###.", "#...#", "#...#", ".####", "....#", "#...#", ".###."]),
        ('A', [".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#"]),
        ('C', [".####", "#....", "#....", "#....", "#....", "#....", ".####"]),
        ('E', ["#####", "#....", "#....", "####.", "#....", "#....", "#####"]),
        ('F', ["#####", "#....", "#....", "####.", "#....", "#....", "#...."]),
        ('I', ["#####", "..#..", "..#..", "..#..", "..#..", "..#..", "#####"]),
        ('K', ["#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#"]),
        ('M', ["#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#"]),
        ('N', ["#...#", "##..#", "##..#", "#.#.#", "#..##", "#..##", "#...#"]),
        ('O', [".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###."]),
        ('R', ["####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#"]),
        ('S', [".####", "#....", "#....", ".###.", "....#", "....#", "####."]),
        ('T', ["#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.."]),
        ('U', ["#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###."]),
        ('W', ["#...#", "#...#", "#...#", "#.#.#", "#.#.#", "##.##", "#...#"]),
        ('X', ["#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#"]),
        ('Y', ["#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.."]),
        ('|', ["..#..", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.."]),
    ];
}

// Builds the RGBA atlas pixels at the fixed grid size; the bar texel
// (a translucent dark blue) occupies cell 0 and glyph pixels are pure
// white over a fully transparent background.

byte[] BuildHudAtlasPixels(
    IReadOnlyDictionary<char, int> cellsByCharacter,
    out int atlasWidth,
    out int atlasHeight)
{
    atlasWidth = HudAtlasColumns * HudCellWidth;

    atlasHeight = HudAtlasRows() * HudCellHeight;

    var pixels = new byte[atlasWidth * atlasHeight * 4];

    var patterns = GetHudGlyphPatterns();

    // Bar texel: centered in cell 0 so quads sample a single texel.

    SetHudTexel(
        pixels,
        atlasWidth,
        HudCellWidth / 2,
        HudCellHeight / 2,
        24,
        34,
        64,
        175);

    for (var i = 0; i < patterns.Length; ++i)
    {
        var glyphCell = cellsByCharacter[patterns[i].Name];

        var cellX = (glyphCell % HudAtlasColumns) * HudCellWidth;
        var cellY = glyphCell / HudAtlasColumns * HudCellHeight;

        for (var row = 0; row < patterns[i].Rows.Length; ++row)
        {
            for (var column = 0; column < patterns[i].Rows[row].Length; ++column)
            {
                if (patterns[i].Rows[row][column] == '#')
                {
                    SetHudTexel(
                        pixels,
                        atlasWidth,
                        cellX + 1 + column,
                        cellY + 1 + row,
                        byte.MaxValue,
                        byte.MaxValue,
                        byte.MaxValue,
                        byte.MaxValue);
                }
            }
        }
    }

    return pixels;
}

void SetHudTexel(
    byte[] pixels,
    int atlasWidth,
    int x,
    int y,
    byte red,
    byte green,
    byte blue,
    byte alpha)
{
    var offset = (y * atlasWidth + x) * 4;

    pixels[offset + 0] = red;
    pixels[offset + 1] = green;
    pixels[offset + 2] = blue;
    pixels[offset + 3] = alpha;
}

// Bakes the HUD help line into a clip-space quad per glyph on the
// fixed interleaved vertex layout. Positions resolve against the
// 1280x720 backbuffer: x maps to [-1, 1] left to right and y to
// [1, -1] top to bottom. Unknown characters fail loudly.

const string HudHelpText = "8 TEXTURES | 9 WIREFRAME | 0 CONTAINERS | S SKY | C CAMERA";

const float HudTextPixelX = 12.0f;

const float HudTextPixelY = 692.0f;

const float HudGlyphPixelScale = 2.0f;

float HudClipX(float pixelX)
{
    return pixelX / 640.0f - 1.0f;
}

float HudClipY(float pixelY)
{
    return 1.0f - pixelY / 360.0f;
}

float[] BuildHudTextVertices(
    string text,
    IReadOnlyDictionary<char, int> cellsByCharacter)
{
    const int stride = Mesh.ComponentsPerVertex;

    var glyphCount = 0;

    foreach (var character in text)
    {
        if (character != ' '
            && !cellsByCharacter.ContainsKey(character))
        {
            throw new ArgumentException(
                $"Character '{character}' has no glyph in the HUD font atlas.");
        }

        if (character != ' ')
        {
            ++glyphCount;
        }
    }

    var vertices = new float[glyphCount * 6 * stride];

    var write = 0;

    var penX = HudTextPixelX;

    var glyphPixelAdvance = 6.0f * HudGlyphPixelScale;

    for (var i = 0; i < text.Length; ++i)
    {
        if (text[i] == ' ')
        {
            penX += glyphPixelAdvance;

            continue;
        }

        var cell = cellsByCharacter[text[i]];

        var column = cell % HudAtlasColumns;
        var row = cell / HudAtlasColumns;

        // Sample texel-center to texel-center: the glyph pattern rows
        // run one texel inside the padded cell, so the outermost glyph
        // texels must not half-blend with the transparent padding.

        var u0 = (column * HudCellWidth + 1.5f) / (HudAtlasColumns * HudCellWidth);
        var u1 = (column * HudCellWidth + 5.5f)
            / (HudAtlasColumns * HudCellWidth);
        var v0 = (row * HudCellHeight + 1.5f)
            / (HudAtlasRows() * HudCellHeight);
        var v1 = (row * HudCellHeight + 7.5f)
            / (HudAtlasRows() * HudCellHeight);

        var x0 = HudClipX(penX);
        var x1 = HudClipX(penX + 5.0f * HudGlyphPixelScale);

        var y0 = HudClipY(HudTextPixelY);
        var y1 = HudClipY(HudTextPixelY + 7.0f * HudGlyphPixelScale);

        // Two triangles covering the glyph quad, corners ordered
        // counter-clockwise, each vertex position, color, normal, uv.

        float[,] corners =
        {
            { x0, y0 },
            { x1, y0 },
            { x1, y1 },
            { x0, y1 },
        };

        float[,] cornerUvs =
        {
            { u0, v0 },
            { u1, v0 },
            { u1, v1 },
            { u0, v1 },
        };

        int[] quadIndices = { 0, 1, 2, 0, 2, 3 };

        foreach (var corner in quadIndices)
        {
            vertices[write + 0] = corners[corner, 0];
            vertices[write + 1] = corners[corner, 1];
            vertices[write + 2] = 0.0f;
            vertices[write + 3] = 1.0f;
            vertices[write + 4] = 1.0f;
            vertices[write + 5] = 1.0f;
            vertices[write + 6] = 0.0f;
            vertices[write + 7] = 0.0f;
            vertices[write + 8] = 1.0f;
            vertices[write + 9] = cornerUvs[corner, 0];
            vertices[write + 10] = cornerUvs[corner, 1];

            write += stride;
        }

        penX += glyphPixelAdvance;
    }

    if (write != vertices.Length)
    {
        throw new InvalidOperationException(
            "HUD glyph mesh build covered unexpected vertex data.");
    }

    return vertices;
}

// Bakes the translucent HUD bar quad that sits behind the help text.

float[] BuildHudBarVertices(
    string text)
{
    const int stride = Mesh.ComponentsPerVertex;

    var barPixelX = HudTextPixelX - 4.0f;
    var barPixelWidth = text.Length * 6.0f * HudGlyphPixelScale + 2.0f;
    var barPixelY = HudTextPixelY - 3.0f;
    var barPixelHeight = 7.0f * HudGlyphPixelScale + 4.0f;

    // The bar quad samples the dedicated bar texel in cell 0.

    var u = HudBarCellU();
    var v = HudBarCellV();

    var x0 = HudClipX(barPixelX);
    var x1 = HudClipX(barPixelX + barPixelWidth);

    var y0 = HudClipY(barPixelY);
    var y1 = HudClipY(barPixelY + barPixelHeight);

    var vertices = new float[6 * stride];

    float[,] corners =
    {
        { x0, y0 },
        { x1, y0 },
        { x1, y1 },
        { x0, y1 },
    };

    int[] quadIndices = { 0, 1, 2, 0, 2, 3 };

    var write = 0;

    foreach (var corner in quadIndices)
    {
        vertices[write + 0] = corners[corner, 0];
        vertices[write + 1] = corners[corner, 1];
        vertices[write + 2] = 0.0f;
        vertices[write + 3] = 1.0f;
        vertices[write + 4] = 1.0f;
        vertices[write + 5] = 1.0f;
        vertices[write + 6] = 0.0f;
        vertices[write + 7] = 0.0f;
        vertices[write + 8] = 1.0f;
        vertices[write + 9] = u;
        vertices[write + 10] = v;

        write += stride;
    }

    return vertices;
}

float HudBarCellU()
{
    return (HudCellWidth / 2.0f) / (HudAtlasColumns * HudCellWidth);
}

float HudBarCellV()
{
    return (HudCellHeight / 2.0f) / (HudAtlasRows() * HudCellHeight);
}

// Total atlas rows: one cell for the bar texel plus one cell per glyph
// character, packed HudAtlasColumns per row.

int HudAtlasRows()
{
    return (HudCharset.Length + 1 + HudAtlasColumns - 1) / HudAtlasColumns;
}

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

// Orbit view over the scene (the original full view). Pulled back to
// frame the glTF model set, which spans several ground units.

_ = CreateCamera(
    new Vector3(0.0f, 2.5f, 8.0f),
    0.0f,
    -0.28f);

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

IShaderProgram? hudShaderProgram = null;

ITexture2D? hudAtlasTexture = null;

IVertexBuffer? hudBarBuffer = null;

IVertexBuffer? hudTextBuffer = null;

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

        // HUD: atlas texture plus the help line quads. The glyph cells
        // map in HudCharset order after the reserved cell 0 (bar).

        var hudCellsByCharacter = new Dictionary<char, int>();

        for (var i = 0; i < HudCharset.Length; ++i)
        {
            hudCellsByCharacter.Add(HudCharset[i], i + 1);
        }

        var hudAtlasPixels = BuildHudAtlasPixels(
            hudCellsByCharacter,
            out var hudAtlasWidth,
            out var hudAtlasHeight);

        hudAtlasTexture = renderer.CreateTexture(
            hudAtlasWidth,
            hudAtlasHeight,
            hudAtlasPixels);

        hudShaderProgram = renderer.CreateShaderProgram(
            HudVertexShaderSource,
            HudFragmentShaderSource);

        hudBarBuffer = renderer.CreateVertexBuffer(
            new Mesh("hud bar", BuildHudBarVertices(HudHelpText)));

        hudTextBuffer = renderer.CreateVertexBuffer(
            new Mesh(
                "hud text",
                BuildHudTextVertices(
                    HudHelpText,
                    hudCellsByCharacter)));

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

        // Model instances: assets under assets/models (instance entries
        // carry their glTF/obj subfolder prefix); the material
        // textures come from assets/textures/materials. Every model
        // file fails loudly when missing or of a wrong format.

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

            var boundsMin = new Vector3(float.MaxValue);
            var boundsMax = new Vector3(float.MinValue);

            foreach (var mesh in model.Meshes)
            {
                totalTriangles += mesh.Indices.Length / 3;

                for (var v = 0; v < mesh.Positions.Length; v += 3)
                {
                    var position = new Vector3(
                        mesh.Positions[v],
                        mesh.Positions[v + 1],
                        mesh.Positions[v + 2]);

                    boundsMin = Vector3.Min(boundsMin, position);
                    boundsMax = Vector3.Max(boundsMax, position);
                }
            }

            Console.WriteLine(
                $"Loaded model '{model.Name}': {model.Meshes.Count} mesh(es), "
                + $"{totalTriangles} triangles, "
                + $"bounds ({boundsMin.X:F1},{boundsMin.Y:F1},{boundsMin.Z:F1}).."
                + $"({boundsMax.X:F1},{boundsMax.Y:F1},{boundsMax.Z:F1}).");

            return model;
        }

        var sceneInstances = new (string ModelFile, string TextureDirectory, string TextureFile, Transform Transform)[]
        {
            // Scales normalize each model's import bounds (logged at
            // load) to comparable scene sizes.

            ("glTF/cow.glb", "rockydirt", "rockydirt-lambertian.png",
                CreateTransform(new Vector3(-3.0f, 0.0f, -1.0f), new Vector3(0.4f), -1.2f, 0.0f)),
            ("glTF/Duck.glb", "stucco", "stucco-lambertian.png",
                CreateTransform(new Vector3(-1.6f, 0.0f, 1.8f), new Vector3(0.8f), 0.5f, 0.0f)),
            ("glTF/Duke.glb", "rustymetal", "rustymetal-lambertian.png",
                CreateTransform(new Vector3(0.9f, 0.0f, 1.6f), new Vector3(1.0f), 2.2f, 0.0f)),
            ("glTF/p-40_warhawk.glb", "metalcrate", "metalcrate-lambertian.png",
                CreateTransform(new Vector3(0.0f, 2.4f, -3.5f), new Vector3(0.35f), -1.9f, 0.15f)),
            ("glTF/star_wars_tie_fighter.glb", "rustymetal", "rustymetal-lambertian.png",
                CreateTransform(new Vector3(-2.6f, 2.6f, -5.5f), new Vector3(0.4f), -0.6f, -0.1f)),
            ("glTF/x_wing.glb", "metalcrate", "metalcrate-lambertian.png",
                CreateTransform(new Vector3(3.5f, 2.2f, -5.0f), new Vector3(15.0f), 0.9f, 0.1f)),
            ("obj/brick_cube.obj", "redbrick", "redbrick-lambertian.png",
                CreateTransform(new Vector3(2.2f, 0.0f, 0.2f), Vector3.One, 0.0f, 0.0f)),
            ("obj/brick_cube.obj", "redbrick", "redbrick-lambertian.png",
                CreateTransform(new Vector3(3.2f, 0.0f, -0.6f), Vector3.One, 0.7f, 0.0f)),
            ("obj/cone.obj", "stucco", "stucco-lambertian.png",
                CreateTransform(new Vector3(4.5f, 0.0f, -3.0f), new Vector3(2.2f, 2.8f, 2.2f), 0.0f, 0.0f)),
            ("obj/sphere.obj", "sand", "sand-lambertian.png",
                CreateTransform(new Vector3(-2.8f, 0.8f, 1.8f), new Vector3(0.8f), 0.0f, 0.0f)),
        };

        foreach (var instance in sceneInstances)
        {
            var model = ImportModel(instance.ModelFile);

            var texture = CreateTextureFromFile(
                Path.Combine(
                    materialTexturesDirectory,
                    instance.TextureDirectory,
                    instance.TextureFile));

            // Per-mesh material textures: meshes whose material carries
            // a texture embedded in the model file (glTF binary) use
            // those pixels; the materials-directory file is the
            // fallback for meshes without embedded textures.

            var embeddedTextures = new Dictionary<int, ITexture2D>();
            var usesEmbeddedTextures = false;

            ITexture2D ResolveTexture(ImportedMesh mesh)
            {
                var embedded = model.Materials.Count > mesh.MaterialIndex
                    ? model.Materials[mesh.MaterialIndex].DiffuseEmbeddedTexture
                    : null;

                if (embedded is null)
                {
                    return texture;
                }

                if (embeddedTextures.TryGetValue(
                        mesh.MaterialIndex,
                        out var existing))
                {
                    return existing;
                }

                var embeddedTexture = renderer.CreateTexture(
                    embedded.Width,
                    embedded.Height,
                    embedded.Pixels);

                embeddedTextures.Add(mesh.MaterialIndex, embeddedTexture);

                usesEmbeddedTextures = true;

                return embeddedTexture;
            }

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
                        ResolveTexture(mesh),
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

            if (usesEmbeddedTextures)
            {
                Console.WriteLine(
                    $"Used embedded textures from '{model.Name}'.");
            }
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

    // HUD: on-screen key options, drawn last in two-dimensional clip
    // space. Depth testing is suspended so the overlay covers the
    // scene, triangles stay filled, and alpha blending mixes the
    // translucent bar and glyph pixels; all state restores after.

    renderer.SetWireframe(false);
    renderer.SetDepthTest(false);
    renderer.SetBlending(true);

    renderer.DrawTriangles(
        hudBarBuffer!,
        hudShaderProgram!,
        "uViewProjection",
        Matrix4x4.Identity,
        hudAtlasTexture!,
        "uTexture");

    renderer.DrawTriangles(
        hudTextBuffer!,
        hudShaderProgram!,
        "uViewProjection",
        Matrix4x4.Identity,
        hudAtlasTexture!,
        "uTexture");

    renderer.SetBlending(false);
    renderer.SetDepthTest(true);

    if (wireframeTriangles)
    {
        renderer.SetWireframe(true);
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
