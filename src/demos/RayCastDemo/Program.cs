// JOT.NET RayCastDemo: Wolfenstein-style 2.5D raycasting after lodev's
// tutorial (lodev.org/cgtutor/raycasting.html). For every screen
// column one ray walks the grid map with DDA steps until it hits a
// wall cell; the slice height comes from the perpendicular distance
// and the column is drawn as a textured stripe: every wall digit
// samples its own texture from assets/textures/materials (decoded
// once at startup through the StbImageLoader). Y-side hits shade to
// 70% so depth still reads, as in the tutorial.
//
// The map loads from an asset file (assets/maps/raycast01.map, a
// plain-text grid) and fails loudly on malformed rows, unknown
// digits, ragged widths, an open border, or a map without open
// cells; wall textures fail loudly on missing or malformed files.
//
// Digit 4 marks enemy spawn cells: open for movement and rays, with
// one enemy sprite placed at the cell's center. Enemies render as
// camera-plane transformed billboards sorted back to front, with the
// wall pass's per-column depth buffer deciding occlusion and an
// alpha test keeping transparent sprite margins clear.
//
// Positional audio rides on the existing Core.Audio glue (as in
// AudioDemo): one looping emitter per enemy at its spawn position and
// the listener synced to the camera every frame, so enemies are heard
// from where they stand — distance attenuates, and left/right follows
// the view direction.
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

using JOT.Core.Audio;
using JOT.Infrastructure.Assimp.Importing;
using JOT.Infrastructure.Silk.Audio;
using JOT.Infrastructure.Silk.Graphics;
using JOT.Infrastructure.Silk.Input;
using JOT.Infrastructure.Silk.Windowing;
using System.Numerics;

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

// Wall textures: every wall digit decodes exactly one texture file at
// startup through the StbImageLoader (RGBA byte rows, any
// dimensions); sampling stays on the CPU, the GL texture only
// carries finished frames. Missing files fail loudly.

var imageLoader = new StbImageLoader();

var wallTextureFiles = new Dictionary<int, string>
{
    [1] = "cement.jpg",
    [2] = "dalbeton.jpg",
    [3] = "brick0.jpg",
};

// Every wall digit present in the map must have a texture assigned
// (digit 4 is an enemy spawn, not a wall); fail loudly at startup
// instead of sampling a null texture later.

for (var row = 0; row < mapRows; ++row)
{
    for (var column = 0; column < mapColumns; ++column)
    {
        var wallDigit = map[row, column];

        if (wallDigit is not (0 or 4) && !wallTextureFiles.ContainsKey(wallDigit))
        {
            throw new InvalidOperationException(
                $"No wall texture is assigned for wall type {wallDigit}.");
        }
    }
}

var textureCount = wallTextureFiles.Keys.Max() + 1;

var texturePixels = new byte[textureCount][];

var textureWidths = new int[textureCount];

var textureHeights = new int[textureCount];

foreach (var wallTexture in wallTextureFiles)
{
    var image = imageLoader.Load(FindTexturePath(wallTexture.Value));

    if (image.Pixels.Length != image.Width * image.Height * 4)
    {
        throw new InvalidOperationException(
            $"{image.SourcePath}: expected RGBA pixels "
            + $"({image.Width}x{image.Height}x4), got "
            + $"{image.Pixels.Length}.");
    }

    texturePixels[wallTexture.Key] = image.Pixels;

    textureWidths[wallTexture.Key] = image.Width;

    textureHeights[wallTexture.Key] = image.Height;

    Console.WriteLine(
        $"Wall texture {wallTexture.Key}: {wallTexture.Value} "
        + $"{image.Width}x{image.Height}.");
}

// Enemy spawns: digit 4 cells are open, and each contributes one
// enemy at its center.

var enemies = new List<(double X, double Y)>();

for (var row = 0; row < mapRows; ++row)
{
    for (var column = 0; column < mapColumns; ++column)
    {
        if (map[row, column] == 4)
        {
            enemies.Add((column + .5, row + .5));
        }
    }
}

Console.WriteLine($"Enemies: {enemies.Count}.");

// Enemy sprite frames decode like the wall textures; the idle frame
// is the first milestone, walk/attack/death frames follow with the
// enemy behaviors.

var enemySpriteImage = imageLoader.Load(
    FindSpritePath("CommandoIdle.png"));

if (enemySpriteImage.Pixels.Length
    != enemySpriteImage.Width * enemySpriteImage.Height * 4)
{
    throw new InvalidOperationException(
        $"{enemySpriteImage.SourcePath}: expected RGBA pixels "
        + $"({enemySpriteImage.Width}x{enemySpriteImage.Height}x4), got "
        + $"{enemySpriteImage.Pixels.Length}.");
}

var enemySpritePixels = enemySpriteImage.Pixels;

var enemySpriteWidth = enemySpriteImage.Width;

var enemySpriteHeight = enemySpriteImage.Height;

Console.WriteLine(
    $"Enemy sprite: CommandoIdle.png "
    + $"{enemySpriteWidth}x{enemySpriteHeight}.");

// Positional audio: one looping emitter per enemy at its spawn
// position; the listener syncs to the camera every frame inside the
// window loop. The damage1 vocal asset stands in for the enemy idle
// sound until the enemies themselves animate (R2c); the first two
// candidates were rejected loudly by WaveFile (a compressed WAV and
// an 8-bit PCM WAV) — honest failures that picked this uncompressed
// 16-bit PCM file instead.

using var audioDevice = new SilkAudioDevice();

var listener = new Listener(audioDevice);

var enemySoundPath = FindSoundPath("Weapons", "damage1.wav");

var enemyWave = WaveFile.Load(enemySoundPath);

var enemyEmitters = new List<SoundEmitter>();

foreach (var enemy in enemies)
{
    var emitter = new SoundEmitter(
        audioDevice,
        enemyWave.Samples,
        enemyWave.SampleRate,
        enemyWave.Channels)
    {
        Position = new Vector3((float)enemy.X, 0.0f, (float)enemy.Y),

        Looping = true,
    };

    emitter.Play();

    enemyEmitters.Add(emitter);
}

Console.WriteLine(
    $"Audio: {enemyEmitters.Count} looping enemy emitters "
    + $"({enemyWave.SampleRate} Hz, {enemyWave.Channels} channel(s)).");

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

    // Digit 4 (enemy spawn) is an open cell too.

    return map[row, column] is 0 or 4;
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

// Locates a wall texture in the materials library by walking up from
// the build output (the convention shared with the map asset).

static string FindTexturePath(string fileName)
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null)
    {
        var candidate = Path.Combine(
            directory.FullName,
            "assets",
            "textures",
            "materials",
            fileName);

        if (File.Exists(candidate))
        {
            return candidate;
        }

        directory = directory.Parent;
    }

    throw new FileNotFoundException(
        "Could not find assets/textures/materials/"
        + fileName
        + " relative to the build output.");
}

// Locates an enemy sprite frame in the 2dot5D sprite library by
// walking up from the build output (the convention shared with the
// map asset and wall textures).

static string FindSpritePath(string fileName)
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null)
    {
        var candidate = Path.Combine(
            directory.FullName,
            "assets",
            "2dot5D",
            "Enemy Characters and Animations",
            fileName);

        if (File.Exists(candidate))
        {
            return candidate;
        }

        directory = directory.Parent;
    }

    throw new FileNotFoundException(
        "Could not find assets/2dot5D/Enemy Characters and Animations/"
        + fileName
        + " relative to the build output.");
}

// Locates a sound in the WAV library by walking up from the build
// output (the convention shared with the other asset lookups).

static string FindSoundPath(string folder, string fileName)
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null)
    {
        var candidate = Path.Combine(
            directory.FullName,
            "assets",
            "sounds",
            folder,
            fileName);

        if (File.Exists(candidate))
        {
            return candidate;
        }

        directory = directory.Parent;
    }

    throw new FileNotFoundException(
        $"Could not find assets/sounds/{folder}/"
        + fileName
        + " relative to the build output.");
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

            if (character is < '0' or > '4')
            {
                throw new InvalidOperationException(
                    $"{path}: line {row + 1}, column {column + 1}: "
                    + $"'{character}' is not a map digit 0-4 "
                    + "(0-3 walls, 4 enemy spawn).");
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

// Per-column wall distance, written by the wall pass and read by the
// sprite pass for occlusion.

var depthBuffer = new double[width];

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

            // Digit 4 is an enemy spawn — an open cell, not a wall;
            // rays pass through it like digit 0.

            if (map[mapY, mapX] is not (0 or 4))
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

        depthBuffer[column] = perpendicular;

        var lineHeight = (int)(height / perpendicular);

        var drawStart = Math.Max(0, height / 2 - lineHeight / 2);

        var drawEnd = Math.Min(height, height / 2 + lineHeight / 2);

        // Horizontal texture coordinate from the fractional wall hit
        // position (the tutorial's formulation); the two sides that
        // would see the texture mirrored have their coordinate
        // flipped.

        var wallHit = side == 0
            ? positionY + perpendicular * rayY
            : positionX + perpendicular * rayX;

        wallHit -= Math.Floor(wallHit);

        var texWidth = textureWidths[wallType];

        var texHeight = textureHeights[wallType];

        var texture = texturePixels[wallType];

        var texX = (int)(wallHit * texWidth);

        if ((side == 0 && rayX > 0) || (side == 1 && rayY < 0))
        {
            texX = texWidth - texX - 1;
        }

        if (texX < 0)
        {
            texX = 0;
        }
        else if (texX >= texWidth)
        {
            texX = texWidth - 1;
        }

        // The vertical coordinate walks the texture in lockstep with
        // the drawn stripe, offset so clipped stripes stay aligned;
        // the clamp tolerates any texture height, not just powers of
        // two.

        var texStep = (double)texHeight / lineHeight;

        var texPos = (drawStart - height / 2.0 + lineHeight / 2.0) * texStep;

        var pixel = drawStart * width + column;

        var end = drawEnd * width + column;

        while (pixel < end)
        {
            var texY = (int)texPos;

            if (texY < 0)
            {
                texY = 0;
            }
            else if (texY >= texHeight)
            {
                texY = texHeight - 1;
            }

            texPos += texStep;

            var texel = (texY * texWidth + texX) * 4;

            if (side == 1)
            {
                framebuffer[pixel * 4 + 0] = (byte)(texture[texel] * 7 / 10);

                framebuffer[pixel * 4 + 1] = (byte)(texture[texel + 1] * 7 / 10);

                framebuffer[pixel * 4 + 2] = (byte)(texture[texel + 2] * 7 / 10);
            }
            else
            {
                framebuffer[pixel * 4 + 0] = texture[texel];

                framebuffer[pixel * 4 + 1] = texture[texel + 1];

                framebuffer[pixel * 4 + 2] = texture[texel + 2];
            }

            framebuffer[pixel * 4 + 3] = byte.MaxValue;

            pixel += width;
        }
    }

    // Sprites: enemies render as camera-plane transformed billboards
    // (the tutorial's formulation), sorted back to front so
    // overlapping enemies paint correctly; the wall pass's per-column
    // depth buffer decides occlusion, and texels alpha-test against
    // half coverage so transparent sprite margins stay clear. The
    // sprite stands on the floor line at 80% of the wall height
    // there.

    var orderedEnemies = enemies
        .OrderByDescending(enemy =>
        {
            var enemyDX = enemy.X - positionX;

            var enemyDY = enemy.Y - positionY;

            return enemyDX * enemyDX + enemyDY * enemyDY;
        })
        .ToList();

    var inverseDet = 1.0 / (planeX * directionY - directionX * planeY);

    foreach (var enemy in orderedEnemies)
    {
        var spriteX = enemy.X - positionX;

        var spriteY = enemy.Y - positionY;

        var transformX = inverseDet
            * (directionY * spriteX - directionX * spriteY);

        var transformY = inverseDet
            * (-planeY * spriteX + planeX * spriteY);

        if (transformY <= 0.05)
        {
            continue;
        }

        var spriteScreenX = (int)
            ((width / 2.0) * (1.0 + transformX / transformY));

        var wallHeight = height / transformY;

        var spriteStripeHeight = (int)(wallHeight * 0.8);

        var spriteStripeWidth = spriteStripeHeight;

        var drawStartY = (int)(height / 2.0 + wallHeight / 2.0)
            - spriteStripeHeight;

        var drawEndY = (int)(height / 2.0 + wallHeight / 2.0);

        var drawStartX = spriteScreenX - spriteStripeWidth / 2;

        var drawEndX = spriteScreenX + spriteStripeWidth / 2;

        var stripeStart = Math.Max(0, drawStartX);

        var stripeEnd = Math.Min(width, drawEndX);

        var rowStart = Math.Max(0, drawStartY);

        var rowEnd = Math.Min(height, drawEndY);

        for (var stripe = stripeStart; stripe < stripeEnd; ++stripe)
        {
            if (transformY >= depthBuffer[stripe])
            {
                continue;
            }

            var texX = (int)
                ((stripe - drawStartX) * enemySpriteWidth
                    / (double)spriteStripeWidth);

            if (texX < 0)
            {
                texX = 0;
            }
            else if (texX >= enemySpriteWidth)
            {
                texX = enemySpriteWidth - 1;
            }

            for (var row = rowStart; row < rowEnd; ++row)
            {
                var texY = (int)
                    ((row - drawStartY) * enemySpriteHeight
                        / (double)spriteStripeHeight);

                if (texY < 0)
                {
                    texY = 0;
                }
                else if (texY >= enemySpriteHeight)
                {
                    texY = enemySpriteHeight - 1;
                }

                var texel = (texY * enemySpriteWidth + texX) * 4;

                if (enemySpritePixels[texel + 3] < 128)
                {
                    continue;
                }

                var target = (row * width + stripe) * 4;

                framebuffer[target + 0] = enemySpritePixels[texel + 0];

                framebuffer[target + 1] = enemySpritePixels[texel + 1];

                framebuffer[target + 2] = enemySpritePixels[texel + 2];

                framebuffer[target + 3] = byte.MaxValue;
            }
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

    // The audio listener follows the camera every frame. The yaw maps
    // the demo's map-space direction (x east, y south) into the audio
    // world so left/right perception matches the view.

    listener.Transform.Position = new Vector3(
        (float)positionX,
        0.0f,
        (float)positionY);

    listener.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(
        MathF.Atan2(-(float)directionX, -(float)directionY),
        0f,
        0f);

    listener.Synchronize();

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
