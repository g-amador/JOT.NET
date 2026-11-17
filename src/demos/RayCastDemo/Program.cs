// JOT.NET RayCastDemo: Wolfenstein-style 2.5D raycasting after lodev's
// tutorial (lodev.org/cgtutor/raycasting.html). For every screen
// column one ray walks the grid map with DDA steps until it hits a
// wall cell; the slice height comes from the perpendicular distance
// and the column is drawn as a flat colored stripe (the first
// milestone — textured wall slices follow). Y-side hits shade to 70%
// so depth still reads without textures, as in the tutorial.
//
// The map loads from an asset file (assets/maps/raycast01.map, a
// plain-text grid) and fails loudly on malformed rows, unknown
// digits, ragged widths, an open border, or a map without open cells.
//
// Movement and turning are keyboard-only for the first milestones
// (mouse look would require new Infrastructure input glue and rides
// along later): the arrows walk and turn, with per-axis move tests
// giving wall sliding against the grid.
//
// Display reuses the proven RayDemo path: a SilkWindow with a
// fullscreen quad textured from the CPU-side RGBA framebuffer; the GL
// texture only carries finished frames, it is never sampled for the
// wall lookups.
//
// The window closes on Escape, after --frames display passes, or
// after --seconds; a one-level 2.5D FPS demo builds on this demo in a
// later step (R2).

using JOT.Infrastructure.Silk.Graphics;
using JOT.Infrastructure.Silk.Input;
using JOT.Infrastructure.Silk.Windowing;

// Render configuration; overridable through the command line.

var width = 480;

var height = 320;

var frameLimit = -1;

var secondsLimit = -1;

for (var i = 0; i < args.Length - 1; ++i)
{
    if (args[i] == "--width")
    {
        width = int.Parse(args[i + 1]);
    }
    else if (args[i] == "--height")
    {
        height = int.Parse(args[i + 1]);
    }
    else if (args[i] == "--frames")
    {
        frameLimit = int.Parse(args[i + 1]);
    }
    else if (args[i] == "--seconds")
    {
        secondsLimit = int.Parse(args[i + 1]);
    }
}

if (width <= 0 || height <= 0)
{
    throw new ArgumentException(
        $"Invalid render resolution: {width}x{height}.");
}

Console.WriteLine($"Raycasting {width}x{height}.");

// The map asset: located by walking up from the build output so the
// harness runs from any working directory (the convention shared
// with AudioDemo).

var mapPath = FindMapPath();

var map = LoadMap(mapPath);

var mapRows = map.GetLength(0);

var mapColumns = map.GetLength(1);

Console.WriteLine(
    $"Map: {mapColumns}x{mapRows} cells ({Path.GetFileName(mapPath)}).");

// Spawn at the center of the first open cell, facing east. The camera
// plane is perpendicular to the direction; its length sets the field
// of view (0.66 is the tutorial's choice, about 66 degrees).

var positionX = 0.0;

var positionY = 0.0;

var spawned = false;

for (var row = 0; row < mapRows && !spawned; ++row)
{
    for (var column = 0; column < mapColumns && !spawned; ++column)
    {
        if (map[row, column] == 0)
        {
            positionX = column + .5;

            positionY = row + .5;

            spawned = true;
        }
    }
}

if (!spawned)
{
    throw new InvalidOperationException(
        "The map has no open cells to spawn in.");
}

var directionX = 1.0;

var directionY = 0.0;

var planeX = 0.0;

var planeY = 0.66;

const double MoveSpeed = 3.0;

const double TurnSpeed = 2.5;

const double CollisionRadius = 0.2;

// One open-cell test for the collision box corners; out-of-bounds
// counts as a wall so a bug can never tunnel out of the map.

bool Open(double x, double y)
{
    var column = (int)x;

    var row = (int)y;

    if (row < 0 || row >= mapRows || column < 0 || column >= mapColumns)
    {
        return false;
    }

    return map[row, column] == 0;
}

// Walkability of a position: the four corners of the collision box
// must all sit in open cells.

bool Walkable(double x, double y)
{
    return Open(x - CollisionRadius, y - CollisionRadius)
        && Open(x + CollisionRadius, y - CollisionRadius)
        && Open(x - CollisionRadius, y + CollisionRadius)
        && Open(x + CollisionRadius, y + CollisionRadius);
}

// Flat wall colors per wall type (the digits documented in the map
// asset); the loader rejects anything outside this set.

static (byte Red, byte Green, byte Blue) WallColor(int type)
{
    return type switch
    {
        1 => (96, 106, 128),
        2 => (70, 160, 96),
        3 => (168, 84, 72),
        _ => throw new InvalidOperationException(
            $"No wall color is defined for wall type {type}."),
    };
}

static string FindMapPath()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null)
    {
        var candidate = Path.Combine(
            directory.FullName,
            "assets",
            "maps",
            "raycast01.map");

        if (File.Exists(candidate))
        {
            return candidate;
        }

        directory = directory.Parent;
    }

    throw new InvalidOperationException(
        "Could not find the assets/maps folder relative to the build "
        + "output.");
}

// Parses the map grid: full-line comments and blank lines are
// skipped; every other line must be the same width of digits 0-3;
// the border must be closed.

static int[,] LoadMap(string path)
{
    var rawLines = File.ReadAllLines(path);

    var lines = new List<string>();

    foreach (var rawLine in rawLines)
    {
        var line = rawLine.TrimEnd();

        if (line.Length == 0 || line.StartsWith('#'))
        {
            continue;
        }

        lines.Add(line);
    }

    if (lines.Count < 3 || lines[0].Length < 3)
    {
        throw new InvalidOperationException(
            $"{path}: the map must be at least 3x3 cells.");
    }

    var width = lines[0].Length;

    var map = new int[lines.Count, width];

    for (var row = 0; row < lines.Count; ++row)
    {
        var line = lines[row];

        if (line.Length != width)
        {
            throw new InvalidOperationException(
                $"{path}: line {row + 1} is {line.Length} cells wide, "
                + $"the map is {width} cells wide.");
        }

        for (var column = 0; column < width; ++column)
        {
            var character = line[column];

            if (character is < '0' or > '3')
            {
                throw new InvalidOperationException(
                    $"{path}: line {row + 1}, column {column + 1}: "
                    + $"'{character}' is not a wall digit 0-3.");
            }

            var border = row == 0 || row == lines.Count - 1
                || column == 0 || column == width - 1;

            if (border && character != '1')
            {
                throw new InvalidOperationException(
                    $"{path}: line {row + 1}, column {column + 1}: the "
                    + "map border must be wall digit 1.");
            }

            map[row, column] = character - '0';
        }
    }

    return map;
}

// The CPU-side RGBA framebuffer; every display pass rewrites it fully.

var framebuffer = new byte[width * height * 4];

var window = new SilkWindow(
    "JOT.NET RayCastDemo — 2.5D raycasting",
    width,
    height);

const string DisplayVertexShaderSource = """
    #version 330 core

    layout (location = 0) in vec3 position;
    layout (location = 1) in vec3 color;
    layout (location = 2) in vec3 normal;
    layout (location = 3) in vec2 uv;

    out vec2 vertexUv;

    void main()
    {
        vertexUv = uv;

        gl_Position = vec4(position.xy, 0.0, 1.0);
    }
    """;

const string DisplayFragmentShaderSource = """
    #version 330 core

    in vec2 vertexUv;

    out vec4 fragmentColor;

    uniform sampler2D uTexture;

    void main()
    {
        fragmentColor = vec4(texture(uTexture, vertexUv).rgb, 1.0);
    }
    """;

IShaderProgram? displayProgram = null;

IVertexBuffer? quadBuffer = null;

ITexture2D? displayTexture = null;

var frameCount = 0;

var stopwatch = System.Diagnostics.Stopwatch.StartNew();

// Draws one full frame into the framebuffer: solid ceiling and floor
// fills, then one DDA ray per column (the tutorial's camera-plane
// formulation) with flat colored, side-shaded wall stripes.

void RenderFrame()
{
    var ceilingRow = height / 2;

    for (var pixel = 0; pixel < ceilingRow * width; ++pixel)
    {
        framebuffer[pixel * 4 + 0] = 52;
        framebuffer[pixel * 4 + 1] = 56;
        framebuffer[pixel * 4 + 2] = 64;
        framebuffer[pixel * 4 + 3] = byte.MaxValue;
    }

    for (var pixel = ceilingRow * width; pixel < width * height; ++pixel)
    {
        framebuffer[pixel * 4 + 0] = 84;
        framebuffer[pixel * 4 + 1] = 76;
        framebuffer[pixel * 4 + 2] = 66;
        framebuffer[pixel * 4 + 3] = byte.MaxValue;
    }

    for (var column = 0; column < width; ++column)
    {
        var cameraX = 2.0 * column / width - 1.0;

        var rayX = directionX + planeX * cameraX;

        var rayY = directionY + planeY * cameraX;

        var mapX = (int)positionX;

        var mapY = (int)positionY;

        var deltaX = rayX == 0 ? 1e30 : Math.Abs(1 / rayX);

        var deltaY = rayY == 0 ? 1e30 : Math.Abs(1 / rayY);

        int stepX;

        int stepY;

        double sideX;

        double sideY;

        if (rayX < 0)
        {
            stepX = -1;

            sideX = (positionX - mapX) * deltaX;
        }
        else
        {
            stepX = 1;

            sideX = (mapX + 1.0 - positionX) * deltaX;
        }

        if (rayY < 0)
        {
            stepY = -1;

            sideY = (positionY - mapY) * deltaY;
        }
        else
        {
            stepY = 1;

            sideY = (mapY + 1.0 - positionY) * deltaY;
        }

        var side = 0;

        var wallType = 0;

        var hit = false;

        while (!hit)
        {
            if (sideX < sideY)
            {
                sideX += deltaX;

                mapX += stepX;

                side = 0;
            }
            else
            {
                sideY += deltaY;

                mapY += stepY;

                side = 1;
            }

            if (mapY < 0 || mapY >= mapRows
                || mapX < 0 || mapX >= mapColumns)
            {
                throw new InvalidOperationException(
                    "A ray left the closed map; the border or spawn is "
                    + "invalid.");
            }

            if (map[mapY, mapX] > 0)
            {
                hit = true;

                wallType = map[mapY, mapX];
            }
        }

        var perpendicular = side == 0
            ? sideX - deltaX
            : sideY - deltaY;

        if (perpendicular < 1e-6)
        {
            perpendicular = 1e-6;
        }

        var lineHeight = (int)(height / perpendicular);

        var drawStart = Math.Max(0, height / 2 - lineHeight / 2);

        var drawEnd = Math.Min(height, height / 2 + lineHeight / 2);

        var (red, green, blue) = WallColor(wallType);

        if (side == 1)
        {
            red = (byte)(red * 7 / 10);

            green = (byte)(green * 7 / 10);

            blue = (byte)(blue * 7 / 10);
        }

        var pixel = drawStart * width + column;

        var end = drawEnd * width + column;

        while (pixel < end)
        {
            framebuffer[pixel * 4 + 0] = red;
            framebuffer[pixel * 4 + 1] = green;
            framebuffer[pixel * 4 + 2] = blue;
            framebuffer[pixel * 4 + 3] = byte.MaxValue;

            pixel += width;
        }
    }
}

window.Run((currentWindow, deltaTime) =>
{
    if (displayProgram is null)
    {
        displayProgram = currentWindow.Graphics.CreateShaderProgram(
            DisplayVertexShaderSource,
            DisplayFragmentShaderSource);

        // Fullscreen quad on the fixed vertex layout (position,
        // color, normal, uv).

        quadBuffer = currentWindow.Graphics.CreateVertexBuffer(
        [
            // position          color                 normal                uv
            -1.0f,  1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f,
             1.0f,  1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
             1.0f, -1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
            -1.0f,  1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f,
             1.0f, -1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
            -1.0f, -1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
        ]);

        currentWindow.Graphics.SetDepthTest(false);
    }

    // Keyboard movement and turning (per-axis move tests give wall
    // sliding against the grid).

    var moveStep = MoveSpeed * deltaTime;

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Up))
    {
        var newX = positionX + directionX * moveStep;

        if (Walkable(newX, positionY))
        {
            positionX = newX;
        }

        var newY = positionY + directionY * moveStep;

        if (Walkable(positionX, newY))
        {
            positionY = newY;
        }
    }

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Down))
    {
        var newX = positionX - directionX * moveStep;

        if (Walkable(newX, positionY))
        {
            positionX = newX;
        }

        var newY = positionY - directionY * moveStep;

        if (Walkable(positionX, newY))
        {
            positionY = newY;
        }
    }

    var turn = 0.0;

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Left))
    {
        turn -= TurnSpeed * deltaTime;
    }

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Right))
    {
        turn += TurnSpeed * deltaTime;
    }

    if (turn != 0.0)
    {
        var cosine = Math.Cos(turn);

        var sine = Math.Sin(turn);

        (directionX, directionY) = (
            directionX * cosine - directionY * sine,
            directionX * sine + directionY * cosine);

        (planeX, planeY) = (
            planeX * cosine - planeY * sine,
            planeX * sine + planeY * cosine);
    }

    RenderFrame();

    displayTexture?.Dispose();

    displayTexture = currentWindow.Graphics.CreateTexture(
        width,
        height,
        framebuffer);

    displayProgram!.Use();

    displayProgram.SetUniform("uTexture", 0);

    displayTexture.Bind(0);

    currentWindow.Graphics.DrawTriangles(quadBuffer!);

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Escape)
        || (frameLimit >= 0 && ++frameCount >= frameLimit)
        || (secondsLimit >= 0 && stopwatch.Elapsed.TotalSeconds >= secondsLimit))
    {
        Console.WriteLine($"Closed at frame {frameCount}.");

        currentWindow.Close();
    }
});
