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
// after --seconds. The one-level 2.5D FPS demo (Step R2 lineage) is
// here: enemies from the map's digit 4 spawns chase the player on
// their walk frames, attack on their attack frames when in range
// (10 damage per attack cycle against the player's 100 HP, and the
// player dying closes the window), and take hitscan shots — Space
// fires (a DDA along the view direction, then the nearest living
// enemy inside the hit width and in front of the wall takes damage,
// flashes its damage frame, and dies through the four death frames).
// Weapons switch with 1 (pistol, PIST1), 2 (machine gun, MCGUN1), and
// 3 (shotgun, Sgun). Each sheet is a flat-background montage whose
// poses are NOT on an even grid, so the loader finds their content
// rectangles; the chroma key skips the sheet's background color and
// the current frame rides bottom-center as an overlay.
// Defeating every enemy prints the win and closes the window.
// --autofire is a validation aid: it keeps aiming at the nearest
// living enemy, walking to clear walls, and firing, so the whole
// combat loop and the win condition can be validated headlessly.
// --weapon picks the starting weapon (0 pistol, 1 machine gun,
// 2 shotgun) so each sheet's draw path can be validated the same way.
//
// Orientation note (fixed 2026-11-17): the fullscreen quad's UVs
// originally mapped framebuffer row 0 to the screen bottom, flipping
// the whole view vertically — invisible on symmetric wall slices, but
// it stood the sprites on the ceiling upside down. The quad now
// samples row 0 at the top.

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

var autofire = args.Contains("--autofire");

var startWeapon = 0;

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
    else if (args[i] == "--weapon")
    {
        startWeapon = int.Parse(args[i + 1]);
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

var enemies = new List<Enemy>();

for (var row = 0; row < mapRows; ++row)
{
    for (var column = 0; column < mapColumns; ++column)
    {
        if (map[row, column] == 4)
        {
            enemies.Add(new Enemy { X = column + .5, Y = row + .5 });
        }
    }
}

Console.WriteLine($"Enemies: {enemies.Count}.");

// Enemy animation frames decode like the wall textures: idle, the
// hit flash, and the four death frames. Every frame must match the
// idle frame's dimensions or the demo fails loudly.

var enemyIdlePixels = DecodeSpriteFrame(
    imageLoader,
    "CommandoIdle.png",
    out var enemySpriteWidth,
    out var enemySpriteHeight);

var enemyDamagePixels = DecodeSpriteFrame(
    imageLoader,
    "CommandoDamage1.png",
    out var damageWidth,
    out var damageHeight);

if (damageWidth != enemySpriteWidth || damageHeight != enemySpriteHeight)
{
    throw new InvalidOperationException(
        "CommandoDamage1.png dimensions differ from CommandoIdle.png.");
}

var enemyDeathFrames = new[]
{
    "CommandoDeath1.png",
    "CommandoDeath2.png",
    "CommandoDeath3.png",
    "CommandoDeath4.png",
}
.Select(fileName =>
{
    var pixels = DecodeSpriteFrame(
        imageLoader,
        fileName,
        out var deathWidth,
        out var deathHeight);

    if (deathWidth != enemySpriteWidth
        || deathHeight != enemySpriteHeight)
    {
        throw new InvalidOperationException(
            $"{fileName} dimensions differ from CommandoIdle.png.");
    }

    return pixels;
})
.ToArray();

Console.WriteLine(
    $"Enemy sprites: idle, damage, and {enemyDeathFrames.Length} "
    + $"death frames ({enemySpriteWidth}x{enemySpriteHeight}).");

// Walking and attacking frame sets complete the enemy animation; every
// frame must match the idle frame's dimensions.

var enemyWalkFrames = new[]
{
    "CommandoWalk1.png",
    "CommandoWalk2.png",
    "CommandoWalk3.png",
    "CommandoWalk4.png",
}
.Select(fileName => DecodeEnemyFrame(
    imageLoader, fileName, enemySpriteWidth, enemySpriteHeight))
.ToArray();

var enemyAttackFrames = new[]
{
    "CommandoAttack1.png",
    "CommandoAttack2.png",
}
.Select(fileName => DecodeEnemyFrame(
    imageLoader, fileName, enemySpriteWidth, enemySpriteHeight))
.ToArray();

Console.WriteLine(
    $"Enemy walk frames: {enemyWalkFrames.Length}; attack frames: "
    + $"{enemyAttackFrames.Length}.");

// Gun sheets: each is a flat opaque background montage of fire poses.
// The nominal grids (PIST1 2x2, MCGUN1 2x2, PIST2 3x2, Sgun 4x5) do
// not align with the art — an even split cut guns in half — so the
// loader detects each pose's content rectangle instead (see
// FindGunFrames). The chroma key makes the sheet's background color
// transparent at draw time; the current frame rides bottom-center as
// the gun overlay.

var weapons = new List<GunSheet>
{
    LoadGunSheet(imageLoader, "Pistol", "PIST1.png", 4, 0.4, 0.07, 1),
    LoadGunSheet(imageLoader, "Machine gun", "MCGUN1.png", 4, 0.15, 0.06, 1),
    // Sgun is a reload montage: only its top row (4 complete firing
    // poses) is the gun's fire animation; the lower rows are hands and
    // shells during reloading. The loader keeps that firing row.
    LoadGunSheet(imageLoader, "Shotgun", "Sgun.png", 4, 0.9, 0.045, 2, 0),
};

foreach (var weapon in weapons)
{
    Console.WriteLine(
        $"Weapon: {weapon.Name} ({weapon.SheetName}, "
        + $"{weapon.Frames.Length} frames, {weapon.FireCooldown:0.00} s "
        + $"cooldown, {weapon.Damage} damage per shot).");
}

var selectedWeapon = autofire ? 1 : startWeapon;

if (selectedWeapon < 0 || selectedWeapon >= weapons.Count)
{
    throw new ArgumentException(
        $"--weapon {startWeapon} is outside 0..{weapons.Count - 1}.");
}

var gunFrame = 0;

var gunFrameTimer = 0.0;

// Positional audio: one looping emitter per enemy at its spawn
// position (kept on the enemy so the death state can silence it); the
// listener syncs to the camera every frame inside the window loop.
// The damage1 vocal asset stands in for the enemy idle sound; the
// first two candidates were rejected loudly by WaveFile (a compressed
// WAV and an 8-bit PCM WAV) — honest failures that picked this
// uncompressed 16-bit PCM file instead. The player's gun shot and the
// enemy hit grunt are one-shot emitters replayed on demand.

using var audioDevice = new SilkAudioDevice();

var listener = new Listener(audioDevice);

var enemyWave = WaveFile.Load(FindSoundPath("Weapons", "damage1.wav"));

foreach (var enemy in enemies)
{
    enemy.Emitter = new SoundEmitter(
        audioDevice,
        enemyWave.Samples,
        enemyWave.SampleRate,
        enemyWave.Channels)
    {
        Position = new Vector3((float)enemy.X, 0.0f, (float)enemy.Y),

        Looping = true,
    };

    enemy.Emitter.Play();
}

Console.WriteLine(
    $"Audio: {enemies.Count} looping enemy emitters "
    + $"({enemyWave.SampleRate} Hz, {enemyWave.Channels} channel(s)).");

var fireWave = WaveFile.Load(FindSoundPath("Weapons", "pdp1_fire.wav"));

var fireEmitter = new SoundEmitter(
    audioDevice,
    fireWave.Samples,
    fireWave.SampleRate,
    fireWave.Channels);

var hitWave = WaveFile.Load(FindSoundPath("Weapons", "damage2.wav"));

var hitEmitter = new SoundEmitter(
    audioDevice,
    hitWave.Samples,
    hitWave.SampleRate,
    hitWave.Channels);

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

const double EnemyMoveSpeed = 1.2;

const double AttackRange = 1.2;

const double AttackCycle = 0.8;

const int AttackDamage = 10;

const int PlayerMaxHealth = 100;

Console.WriteLine(
    $"Player: {PlayerMaxHealth} HP; an enemy in range deals "
    + $"{AttackDamage} damage every {AttackCycle:0.0} s.");

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

// Locates a sprite frame under the 2dot5D sprite library by walking
// up from the build output (the convention shared with the other
// asset lookups).

static string FindSpritePath(string folder, string fileName)
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null)
    {
        var candidate = Path.Combine(
            directory.FullName,
            "assets",
            "2dot5D",
            folder,
            fileName);

        if (File.Exists(candidate))
        {
            return candidate;
        }

        directory = directory.Parent;
    }

    throw new FileNotFoundException(
        $"Could not find assets/2dot5D/{folder}/"
        + fileName
        + " relative to the build output.");
}

// Decodes one sprite frame and requires RGBA rows; the frame's
// dimensions come back through the out parameters so callers can
// cross-check them against the rest of the animation set.

static byte[] DecodeSpriteFrame(
    StbImageLoader loader,
    string fileName,
    out int frameWidth,
    out int frameHeight)
{
    return DecodeImage(
        loader,
        FindSpritePath("Enemy Characters and Animations", fileName),
        out frameWidth,
        out frameHeight);
}

static byte[] DecodeImage(
    StbImageLoader loader,
    string path,
    out int imageWidth,
    out int imageHeight)
{
    var image = loader.Load(path);

    if (image.Pixels.Length != image.Width * image.Height * 4)
    {
        throw new InvalidOperationException(
            $"{path}: expected RGBA pixels "
            + $"({image.Width}x{image.Height}x4), got "
            + $"{image.Pixels.Length}.");
    }

    imageWidth = image.Width;

    imageHeight = image.Height;

    return image.Pixels;
}

// Decodes one enemy frame and requires it to match the idle frame's
// dimensions (every Commando frame shares one 64x64 canvas).

static byte[] DecodeEnemyFrame(
    StbImageLoader loader,
    string fileName,
    int expectedWidth,
    int expectedHeight)
{
    var pixels = DecodeSpriteFrame(
        loader,
        fileName,
        out var frameWidth,
        out var frameHeight);

    if (frameWidth != expectedWidth || frameHeight != expectedHeight)
    {
        throw new InvalidOperationException(
            $"{fileName}: expected {expectedWidth}x{expectedHeight}, "
            + $"got {frameWidth}x{frameHeight}.");
    }

    return pixels;
}

// Decodes a gun sprite sheet, verifies its corners share one flat
// background color (the chroma key depends on it), and finds the real
// frame rectangles from the artwork. The sheets are hand-packed
// montages — their poses are NOT on an even grid (an even split cut
// guns in half), so the loader masks the background, labels connected
// content blobs, drops small blobs (ejected shells), groups the rest
// into rows by vertical proximity, and orders them row-major.
// Frames are sliced at draw time, so no per-frame copies are made.

static GunSheet LoadGunSheet(
    StbImageLoader loader,
    string name,
    string fileName,
    int expectedFrames,
    double fireCooldown,
    double frameTime,
    int damage,
    int? onlyRow = null)
{
    var pixels = DecodeImage(
        loader,
        FindSpritePath("Gun Sprites", fileName),
        out var width,
        out var height);

    var backgroundRed = pixels[0];

    var backgroundGreen = pixels[1];

    var backgroundBlue = pixels[2];

    var bottomLeft = (height - 1) * width * 4;

    var bottomRight = bottomLeft + (width - 1) * 4;

    var topRight = (width - 1) * 4;

    foreach (var corner in new[] { topRight, bottomLeft, bottomRight })
    {
        if (pixels[corner] != backgroundRed
            || pixels[corner + 1] != backgroundGreen
            || pixels[corner + 2] != backgroundBlue)
        {
            throw new InvalidOperationException(
                fileName + ": the sheet corners do not share one "
                + "background color; the chroma key cannot be trusted.");
        }
    }

    var frames = FindGunFrames(
        pixels,
        width,
        height,
        backgroundRed,
        backgroundGreen,
        backgroundBlue,
        fileName);

    var detectedCount = frames.Length;

    if (onlyRow is int row)
    {
        frames = frames.Where(frame => frame.Row == row).ToArray();
    }

    if (frames.Length != expectedFrames)
    {
        throw new InvalidOperationException(
            $"{fileName}: {detectedCount} gun poses detected, "
            + $"{frames.Length} after the row filter, "
            + $"expected {expectedFrames}.");
    }

    var rowCount = frames.Select(frame => frame.Row).Distinct().Count();

    var referenceHeight = frames
        .Select(frame => frame.Height)
        .OrderBy(frameHeight => frameHeight)
        .ElementAt(frames.Length / 2);

    Console.WriteLine(
        $"{fileName}: {frames.Length} frames in {rowCount} row"
        + $"{(rowCount == 1 ? string.Empty : "s")} "
        + "(content rectangles, row-major).");

    return new GunSheet
    {
        Name = name,
        SheetName = fileName,
        Pixels = pixels,
        Width = width,
        Height = height,
        Frames = frames,
        ReferenceHeight = referenceHeight,
        FireCooldown = fireCooldown,
        FrameTime = frameTime,
        Damage = damage,
        BackgroundRed = backgroundRed,
        BackgroundGreen = backgroundGreen,
        BackgroundBlue = backgroundBlue,
    };
}

// Finds one rectangle per gun pose: background-masked connected
// components (8-neighbour) of at least 2000 pixels are the guns; the
// sub-2000 leftovers are ejected shells or hands and are ignored. The
// surviving blobs are banded into rows by their vertical centre (a gap
// wider than 45% of the median blob height starts a new row), sorted
// left to right inside their row, and finally verified against the
// frame count the caller declared.

static FrameRect[] FindGunFrames(
    byte[] pixels,
    int width,
    int height,
    int backgroundRed,
    int backgroundGreen,
    int backgroundBlue,
    string fileName)
{
    var mask = new bool[width * height];

    for (var y = 0; y < height; ++y)
    {
        for (var x = 0; x < width; ++x)
        {
            var texel = (y * width + x) * 4;

            var redDelta = pixels[texel] - backgroundRed;

            var greenDelta = pixels[texel + 1] - backgroundGreen;

            var blueDelta = pixels[texel + 2] - backgroundBlue;

            mask[y * width + x] = redDelta * redDelta
                + greenDelta * greenDelta
                + blueDelta * blueDelta > 2500;
        }
    }

    var visited = new bool[mask.Length];

    var queue = new int[mask.Length];

    var blobs = new List<(int X0, int Y0, int X1, int Y1, int Pixels)>();

    for (var start = 0; start < mask.Length; ++start)
    {
        if (!mask[start] || visited[start])
        {
            continue;
        }

        var head = 0;

        var tail = 0;

        queue[tail++] = start;

        visited[start] = true;

        var x0 = start % width;

        var y0 = start / width;

        var x1 = x0;

        var y1 = y0;

        var count = 0;

        while (head < tail)
        {
            var index = queue[head++];

            var x = index % width;

            var y = index / width;

            ++count;

            if (x < x0) x0 = x;
            if (x > x1) x1 = x;
            if (y < y0) y0 = y;
            if (y > y1) y1 = y;

            for (var dy = -1; dy <= 1; ++dy)
            {
                for (var dx = -1; dx <= 1; ++dx)
                {
                    var neighbourX = x + dx;

                    var neighbourY = y + dy;

                    if (neighbourX < 0 || neighbourX >= width
                        || neighbourY < 0 || neighbourY >= height)
                    {
                        continue;
                    }

                    var neighbour = neighbourY * width + neighbourX;

                    if (mask[neighbour] && !visited[neighbour])
                    {
                        visited[neighbour] = true;

                        queue[tail++] = neighbour;
                    }
                }
            }
        }

        if (count >= 2000)
        {
            blobs.Add((x0, y0, x1, y1, count));
        }
    }

    if (blobs.Count == 0)
    {
        throw new InvalidOperationException(
            fileName + ": no gun poses found above the flat background.");
    }

    var medianHeight = blobs
        .Select(blob => blob.Y1 - blob.Y0 + 1)
        .OrderBy(blobHeight => blobHeight)
        .ElementAt(blobs.Count / 2);

    var rowGap = medianHeight * 0.45;

    var rows = new List<List<(int X0, int Y0, int X1, int Y1, int Pixels)>>();

    var bandMaxCentre = double.NegativeInfinity;

    foreach (var blob in blobs.OrderBy(
        blob => (blob.Y0 + blob.Y1) / 2.0))
    {
        var centre = (blob.Y0 + blob.Y1) / 2.0;

        if (rows.Count == 0 || centre - bandMaxCentre > rowGap)
        {
            rows.Add(new List<(int, int, int, int, int)>());

            bandMaxCentre = centre;
        }
        else
        {
            bandMaxCentre = Math.Max(bandMaxCentre, centre);
        }

        rows[^1].Add(blob);
    }

    var frames = new List<FrameRect>();

    for (var row = 0; row < rows.Count; ++row)
    {
        foreach (var blob in rows[row].OrderBy(blob => blob.X0))
        {
            var frameWidth = blob.X1 - blob.X0 + 1;

            var frameHeight = blob.Y1 - blob.Y0 + 1;

            // The anchor is the horizontal centre of the bottom fifth of
            // the content (the grip or arm), so the muzzle flash above
            // never shifts the gun sideways between frames.
            var anchorTop = blob.Y1 - frameHeight / 5;

            var anchorSum = 0;

            var anchorCount = 0;

            for (var y = Math.Max(blob.Y0, anchorTop); y <= blob.Y1; ++y)
            {
                for (var x = blob.X0; x <= blob.X1; ++x)
                {
                    if (mask[y * width + x])
                    {
                        anchorSum += x;

                        ++anchorCount;
                    }
                }
            }

            frames.Add(new FrameRect
            {
                X = blob.X0,
                Y = blob.Y0,
                Width = frameWidth,
                Height = frameHeight,
                AnchorX = anchorCount == 0
                    ? frameWidth / 2.0
                    : anchorSum / (double)anchorCount - blob.X0,
                Row = row,
            });
        }
    }

    if (frames.Count == 0)
    {
        throw new InvalidOperationException(
            fileName + ": no gun poses found above the flat background.");
    }

    return frames.ToArray();
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

var fireCooldown = 0.0;

var playerHealth = PlayerMaxHealth;

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
    // there. Each enemy shows the frame its animation state selects
    // (walk, attack, hit flash, or a death frame); the dead render
    // nothing.

    var orderedEnemies = enemies
        .Where(enemy => enemy.State is not "Dead")
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

        var sprite = enemy.State switch
        {
            "Damage" => enemyDamagePixels,

            "Dying" => enemyDeathFrames[enemy.DeathFrame],

            "Dead" => enemyDeathFrames[enemyDeathFrames.Length - 1],

            "Attack" => enemyAttackFrames[
                (int)(enemy.StateTimer / 0.4)
                    % enemyAttackFrames.Length],

            "Walk" => enemyWalkFrames[enemy.WalkFrame],

            _ => enemyIdlePixels,
        };

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

                if (sprite[texel + 3] < 128)
                {
                    continue;
                }

                var target = (row * width + stripe) * 4;

                framebuffer[target + 0] = sprite[texel + 0];

                framebuffer[target + 1] = sprite[texel + 1];

                framebuffer[target + 2] = sprite[texel + 2];

                framebuffer[target + 3] = byte.MaxValue;
            }
        }
    }

    // Player health bar: a dark backdrop with a red fill proportional
    // to the remaining health reads at a glance while enemies close in.

    var healthBarWidth = Math.Max(1, (int)(width * 0.3));

    var healthBarHeight = Math.Max(3, height / 50);

    for (var row = 8; row < 8 + healthBarHeight && row < height; ++row)
    {
        for (var column = 8;
            column < 8 + healthBarWidth && column < width;
            ++column)
        {
            var target = (row * width + column) * 4;

            var filled = column - 8
                < healthBarWidth * Math.Max(0, playerHealth) / PlayerMaxHealth;

            framebuffer[target + 0] = filled ? (byte)200 : (byte)45;

            framebuffer[target + 1] = filled ? (byte)40 : (byte)45;

            framebuffer[target + 2] = filled ? (byte)40 : (byte)45;

            framebuffer[target + 3] = byte.MaxValue;
        }
    }

    // Gun overlay: the equipped weapon's current fire frame rides
    // bottom-center of the view. Frames are the sheet's detected
    // content rectangles, scaled by one shared factor (the median
    // frame height) so the gun body keeps its size while a muzzle
    // flash extends upward, and anchored by the grip's horizontal
    // centre so the gun does not sway between frames. The chroma key
    // skips every texel that matches the sheet's flat background
    // color.

    var gun = weapons[selectedWeapon];

    var gunFrameRect = gun.Frames[
        Math.Min(gunFrame, gun.Frames.Length - 1)];

    var gunStripeHeight = (int)(height * 0.6);

    var gunScale = gunStripeHeight / gun.ReferenceHeight;

    var gunDrawWidth = Math.Max(
        1, (int)Math.Round(gunFrameRect.Width * gunScale));

    var gunDrawHeight = Math.Max(
        1, (int)Math.Round(gunFrameRect.Height * gunScale));

    var gunStartX = width / 2
        - (int)Math.Round(gunFrameRect.AnchorX * gunScale);

    var gunStartY = height - gunDrawHeight;

    for (var stripe = Math.Max(0, gunStartX);
        stripe < Math.Min(width, gunStartX + gunDrawWidth);
        ++stripe)
    {
        var gunTexX = gunFrameRect.X + (int)
            ((stripe - gunStartX) * gunFrameRect.Width
                / (double)gunDrawWidth);

        if (gunTexX < gunFrameRect.X)
        {
            gunTexX = gunFrameRect.X;
        }
        else if (gunTexX >= gunFrameRect.X + gunFrameRect.Width)
        {
            gunTexX = gunFrameRect.X + gunFrameRect.Width - 1;
        }

        for (var row = Math.Max(0, gunStartY); row < height; ++row)
        {
            var gunTexY = gunFrameRect.Y + (int)
                ((row - gunStartY) * gunFrameRect.Height
                    / (double)gunDrawHeight);

            if (gunTexY < gunFrameRect.Y)
            {
                gunTexY = gunFrameRect.Y;
            }
            else if (gunTexY >= gunFrameRect.Y + gunFrameRect.Height)
            {
                gunTexY = gunFrameRect.Y + gunFrameRect.Height - 1;
            }

            var gunTexel = (gunTexY * gun.Width + gunTexX) * 4;

            var redDelta = gun.Pixels[gunTexel] - gun.BackgroundRed;

            var greenDelta = gun.Pixels[gunTexel + 1]
                - gun.BackgroundGreen;

            var blueDelta = gun.Pixels[gunTexel + 2]
                - gun.BackgroundBlue;

            if (redDelta * redDelta + greenDelta * greenDelta
                + blueDelta * blueDelta <= 2500)
            {
                continue;
            }

            var gunTarget = (row * width + stripe) * 4;

            framebuffer[gunTarget + 0] = gun.Pixels[gunTexel + 0];

            framebuffer[gunTarget + 1] = gun.Pixels[gunTexel + 1];

            framebuffer[gunTarget + 2] = gun.Pixels[gunTexel + 2];

            framebuffer[gunTarget + 3] = byte.MaxValue;
        }
    }
}

// The nearest enemy still able to take damage (validation aid and the
// future homing target for enemy AI).

Enemy? NearestLivingEnemy()
{
    Enemy? nearest = null;

    var nearestSquare = double.MaxValue;

    foreach (var enemy in enemies)
    {
        if (enemy.State is "Dying" or "Dead")
        {
            continue;
        }

        var enemyDX = enemy.X - positionX;

        var enemyDY = enemy.Y - positionY;

        var square = enemyDX * enemyDX + enemyDY * enemyDY;

        if (square < nearestSquare)
        {
            nearestSquare = square;

            nearest = enemy;
        }
    }

    return nearest;
}

// Aims the camera straight at an enemy: the direction turns toward it
// and the camera plane stays its perpendicular half-FOV vector.

void AimAt(Enemy target)
{
    var aimX = target.X - positionX;

    var aimY = target.Y - positionY;

    var aimLength = Math.Sqrt(aimX * aimX + aimY * aimY);

    if (aimLength < 1e-6)
    {
        return;
    }

    directionX = aimX / aimLength;

    directionY = aimY / aimLength;

    planeX = -directionY * 0.66;

    planeY = directionX * 0.66;
}

// Fires one hitscan shot: the gunshot sound plays at the listener,
// the exact view-direction ray gives the wall distance (the DDA with
// cameraX = 0), and the nearest living enemy inside its billboard
// half-width and in front of the wall takes the damage — a hit flash
// while hit points remain, the death sequence at zero.

void FireWeapon(IWindow currentWindow)
{
    fireEmitter.Stop();

    fireEmitter.Position = listener.Transform.Position;

    fireEmitter.Play();

    var wallDepth = WallDistance();

    var inverseDet = 1.0 / (planeX * directionY - directionX * planeY);

    Enemy? target = null;

    var targetDepth = wallDepth;

    foreach (var enemy in enemies)
    {
        if (enemy.State is "Dying" or "Dead")
        {
            continue;
        }

        var spriteX = enemy.X - positionX;

        var spriteY = enemy.Y - positionY;

        var transformY = inverseDet
            * (-planeY * spriteX + planeX * spriteY);

        var transformX = inverseDet
            * (directionY * spriteX - directionX * spriteY);

        if (transformY > 0.2
            && transformY < targetDepth
            && Math.Abs(transformX) < 0.4)
        {
            target = enemy;

            targetDepth = transformY;
        }
    }

    if (target is null)
    {
        return;
    }

    target.HitPoints -= weapons[selectedWeapon].Damage;

    if (target.HitPoints > 0)
    {
        target.State = "Damage";

        target.StateTimer = 0.0;

        hitEmitter.Stop();

        hitEmitter.Position = new Vector3(
            (float)target.X,
            0.0f,
            (float)target.Y);

        hitEmitter.Play();
    }
    else
    {
        target.State = "Dying";

        target.StateTimer = 0.0;

        target.DeathFrame = 0;

        var remaining = enemies.Count(
            enemy => enemy.State is not ("Dying" or "Dead"));

        Console.WriteLine(
            $"Enemy defeated; {remaining} remain"
            + (remaining == 1 ? "s." : "."));
    }
}

// Whether the aimed camera actually sees the enemy: the enemy must be
// in front, inside its billboard half-width, and nearer than the wall
// along the exact view direction.

bool HasLineOfSight(Enemy enemy)
{
    var inverseDet = 1.0 / (planeX * directionY - directionX * planeY);

    var spriteX = enemy.X - positionX;

    var spriteY = enemy.Y - positionY;

    var transformY = inverseDet
        * (-planeY * spriteX + planeX * spriteY);

    var transformX = inverseDet
        * (directionY * spriteX - directionX * spriteY);

    return transformY > 0.2
        && Math.Abs(transformX) < 0.4
        && transformY < WallDistance();
}

// The wall distance along the exact view direction: the same DDA walk
// the wall pass runs, but for the single center ray.

double WallDistance()
{
    var rayX = directionX;

    var rayY = directionY;

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

    while (true)
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
                "A shot ray left the closed map; the border or spawn "
                + "is invalid.");
        }

        if (map[mapY, mapX] is not (0 or 4))
        {
            break;
        }
    }

    var perpendicular = side == 0
        ? sideX - deltaX
        : sideY - deltaY;

    if (perpendicular < 0.1)
    {
        perpendicular = 0.1;
    }

    return perpendicular;
}

window.Run((currentWindow, deltaTime) =>
{
    if (displayProgram is null)
    {
        displayProgram = currentWindow.Graphics.CreateShaderProgram(
            DisplayVertexShaderSource,
            DisplayFragmentShaderSource);

        // Fullscreen quad on the fixed vertex layout (position,
        // color, normal, uv). The v coordinate maps framebuffer
        // row 0 to the screen top; the original mapping sampled row
        // 0 at the bottom and displayed the whole view flipped.

        quadBuffer = currentWindow.Graphics.CreateVertexBuffer(
        [
            // position          color                 normal                uv
            -1.0f,  1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
             1.0f,  1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
             1.0f, -1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
            -1.0f,  1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
             1.0f, -1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
            -1.0f, -1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f,
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

    // The audio listener follows the camera every frame, after
    // movement and turning so it carries the fresh pose.

    listener.Transform.Position = new Vector3(
        (float)positionX,
        0.0f,
        (float)positionY);

    listener.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(
        MathF.Atan2(-(float)directionX, -(float)directionY),
        0f,
        0f);

    listener.Synchronize();

    // Enemy animation and behaviour tick with the frame time. A hit
    // flash returns to walking; a walking enemy always closes on the
    // player (per-axis collision sliding, four walk frames), and an
    // enemy inside attack range switches to the two attack frames and
    // lands 10 damage every attack cycle. At zero health the player
    // dies and the window closes. The death sequence advances through
    // its frames, silences the emitter on the final one, and defeating
    // the last enemy is the win condition.

    foreach (var enemy in enemies)
    {
        if (enemy.State == "Damage")
        {
            enemy.StateTimer += deltaTime;

            if (enemy.StateTimer >= 0.25)
            {
                enemy.State = "Walk";
            }
        }
        else if (enemy.State == "Dying")
        {
            enemy.StateTimer += deltaTime;

            while (enemy.StateTimer >= 0.12
                && enemy.DeathFrame < enemyDeathFrames.Length - 1)
            {
                enemy.StateTimer -= 0.12;

                enemy.DeathFrame++;
            }

            if (enemy.DeathFrame == enemyDeathFrames.Length - 1
                && enemy.StateTimer >= 0.12)
            {
                enemy.State = "Dead";

                enemy.Emitter?.Stop();

                if (enemies.All(defeated => defeated.State == "Dead"))
                {
                    Console.WriteLine("All enemies defeated — you win!");

                    currentWindow.Close();
                }
            }
        }
        else if (enemy.State is "Walk" or "Attack")
        {
            var toPlayerX = positionX - enemy.X;

            var toPlayerY = positionY - enemy.Y;

            var playerDistance = Math.Sqrt(
                toPlayerX * toPlayerX + toPlayerY * toPlayerY);

            if (enemy.State == "Attack" && playerDistance > AttackRange * 1.25)
            {
                enemy.State = "Walk";

                enemy.StateTimer = 0.0;
            }

            if (enemy.State == "Walk")
            {
                if (playerDistance <= AttackRange)
                {
                    enemy.State = "Attack";

                    enemy.StateTimer = 0.0;
                }
                else if (playerDistance >= 1e-6)
                {
                    var chaseStep = EnemyMoveSpeed * deltaTime;

                    var chaseX = toPlayerX / playerDistance * chaseStep;

                    if (Walkable(enemy.X + chaseX, enemy.Y))
                    {
                        enemy.X += chaseX;
                    }

                    var chaseY = toPlayerY / playerDistance * chaseStep;

                    if (Walkable(enemy.X, enemy.Y + chaseY))
                    {
                        enemy.Y += chaseY;
                    }

                    enemy.WalkTimer += deltaTime;

                    while (enemy.WalkTimer >= 0.15)
                    {
                        enemy.WalkTimer -= 0.15;

                        enemy.WalkFrame = (enemy.WalkFrame + 1)
                            % enemyWalkFrames.Length;
                    }
                }

                if (enemy.Emitter is not null)
                {
                    enemy.Emitter.Position = new Vector3(
                        (float)enemy.X,
                        0.0f,
                        (float)enemy.Y);
                }
            }
            else
            {
                enemy.StateTimer += deltaTime;

                while (enemy.StateTimer >= AttackCycle)
                {
                    enemy.StateTimer -= AttackCycle;

                    playerHealth -= AttackDamage;

                    Console.WriteLine(
                        $"An enemy hits you for {AttackDamage}; "
                        + $"{Math.Max(0, playerHealth)} HP left.");

                    if (playerHealth <= 0)
                    {
                        Console.WriteLine("You died.");

                        currentWindow.Close();

                        return;
                    }
                }
            }
        }
    }

    // Weapon switching: 1 selects the pistol, 2 the machine gun, and 3
    // the shotgun. Switching returns the gun to its idle frame (frame
    // 0) and clears the cooldown so the first shot is immediate.

    var previousWeapon = selectedWeapon;

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Number1))
    {
        selectedWeapon = 0;
    }
    else if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Number2))
    {
        selectedWeapon = 1;
    }
    else if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Number3))
    {
        selectedWeapon = 2;
    }

    if (selectedWeapon != previousWeapon)
    {
        gunFrame = 0;

        gunFrameTimer = 0.0;

        fireCooldown = 0.0;

        Console.WriteLine($"Switched to {weapons[selectedWeapon].Name}.");
    }

    // Gun animation: a shot starts the sheet's frame sequence (frame 0
    // is the idle pose) and the sequence returns to idle after the last
    // frame.

    if (gunFrame > 0)
    {
        gunFrameTimer += deltaTime;

        var gunSheet = weapons[selectedWeapon];

        while (gunFrameTimer >= gunSheet.FrameTime)
        {
            gunFrameTimer -= gunSheet.FrameTime;

            if (++gunFrame >= gunSheet.Frames.Length)
            {
                gunFrame = 0;

                gunFrameTimer = 0.0;

                break;
            }
        }
    }

    // Shooting: Space fires hitscan shots on the equipped weapon's
    // cooldown; --autofire is the validation aid that aims at the
    // nearest living enemy, walks toward it when a wall blocks the
    // view, and fires once the line of sight is clear, so the combat
    // loop and the win condition validate headlessly. It holds the
    // machine gun so the run finishes inside the player's health
    // budget.

    var autoFireReady = false;

    if (autofire)
    {
        var autoTarget = NearestLivingEnemy();

        if (autoTarget is not null)
        {
            AimAt(autoTarget);

            if (HasLineOfSight(autoTarget))
            {
                autoFireReady = true;
            }
            else
            {
                // Walk toward the target with the same per-axis
                // collision sliding the player uses; pillars slide
                // past once the corridor opens.

                var toX = autoTarget.X - positionX;

                var toY = autoTarget.Y - positionY;

                var toLength = Math.Sqrt(toX * toX + toY * toY);

                if (toLength >= 1e-6)
                {
                    var walkStep = MoveSpeed * deltaTime;

                    var stepX = toX / toLength * walkStep;

                    if (Walkable(positionX + stepX, positionY))
                    {
                        positionX += stepX;
                    }

                    var stepY = toY / toLength * walkStep;

                    if (Walkable(positionX, positionY + stepY))
                    {
                        positionY += stepY;
                    }
                }
            }
        }
    }

    fireCooldown -= deltaTime;

    if ((autoFireReady
            || (!autofire
                && currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Space)))
        && fireCooldown <= 0.0)
    {
        fireCooldown = weapons[selectedWeapon].FireCooldown;

        gunFrame = 1;

        gunFrameTimer = 0.0;

        FireWeapon(currentWindow);
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

    ++frameCount;

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Escape)
        || (frameLimit >= 0 && frameCount >= frameLimit)
        || (secondsLimit >= 0 && stopwatch.Elapsed.TotalSeconds >= secondsLimit))
    {
        Console.WriteLine($"Closed at frame {frameCount}.");

        currentWindow.Close();
    }
});

// One enemy: map position, hit points, animation state (Walk, Attack,
// Damage hit flash, Dying through the death frames, Dead), animation
// clocks, and the positional emitter carrying its idle sound.

internal sealed class Enemy
{
    public double X { get; set; }

    public double Y { get; set; }

    public int HitPoints { get; set; } = 2;

    public string State { get; set; } = "Walk";

    public double StateTimer { get; set; }

    public int WalkFrame { get; set; }

    public double WalkTimer { get; set; }

    public int DeathFrame { get; set; }

    public SoundEmitter? Emitter { get; set; }
}

// One gun sprite sheet: the decoded pixels stay whole; Frames holds the
// content rectangles found at load time (row-major, top row first).
// ReferenceHeight is the median frame height behind the shared draw
// scale, and the background color is the chroma key the overlay skips.

internal sealed class GunSheet
{
    public required string Name { get; init; }

    public required string SheetName { get; init; }

    public required byte[] Pixels { get; init; }

    public required int Width { get; init; }

    public required int Height { get; init; }

    public required FrameRect[] Frames { get; init; }

    public required double ReferenceHeight { get; init; }

    public required double FireCooldown { get; init; }

    public required double FrameTime { get; init; }

    public required int Damage { get; init; }

    public required int BackgroundRed { get; init; }

    public required int BackgroundGreen { get; init; }

    public required int BackgroundBlue { get; init; }
}

// One detected gun pose inside a sheet: its content rectangle and the
// horizontal anchor (the grip's centre) used to keep the gun steady
// while the muzzle flash changes the rectangle's size.

internal sealed class FrameRect
{
    public required int X { get; init; }

    public required int Y { get; init; }

    public required int Width { get; init; }

    public required int Height { get; init; }

    public required double AnchorX { get; init; }

    public required int Row { get; init; }
}
