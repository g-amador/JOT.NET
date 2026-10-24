// JOT.NET RayDemo: a smallpt-faithful CPU Monte Carlo path tracer,
// previewed live in an engine window (after Kevin Beason's 99-line
// smallpt, 2008).
//
// Scene: the modified Cornell box from smallpt, with its original
// constants — a diffuse luminaire on the ceiling between a red and a
// blue wall, a mirror sphere and a glass sphere on a white floor,
// viewed by a camera outside the room. The six walls are spheres of
// radius 1e5, so ray-sphere intersection covers the whole scene.
//
// Every statistically material feature of smallpt is ported, plus its
// two published modifications (kevinbeason.com/smallpt/#mods):
//
//  - Global illumination via unbiased Monte Carlo path tracing.
//  - Ray-sphere intersection for the entire scene description.
//  - Ideal diffuse, ideal specular, and dielectric glass BRDFs.
//  - Cosine importance sampling of the hemisphere for diffuse
//    reflection.
//  - Soft shadows from the diffuse luminaire via explicit light
//    sampling (the first modification): direct radiance from diffuse
//    vertices is concentrated by solid-angle sampling of the lamp
//    (cone sampling over the sub-tended solid angle); the resulting
//    indirect bounce suppresses the lamp's emission so the estimator
//    stays unbiased (nothing is double counted).
//  - Antialiasing via super-sampling with an importance-sampled tent
//    distribution across 2x2 subpixels (four strata per pixel).
//  - Russian roulette terminates paths deeper than five bounces with
//    a probability matching the largest albedo channel.
//  - Russian roulette and splitting for selecting reflection and/or
//    refraction of the glass BRDF: both branches are traced weighted
//    by their Fresnel probabilities while the path is shallow, then
//    share one ray through Fresnel-probability Russian roulette.
//  - Non-branching ray tree (the second modification): deeper glass
//    paths scatter through a single Fresnel-probability ray, and the
//    diffuse bounce continues one cosine-sampled indirect ray.
//
// Parallelism: a background render task advances one full pass (one
// radiance sample per subpixel) at a time, striped over all logical
// cores — the C# counterpart of smallpt's OpenMP row loop. The window
// shows the running mean converging toward the final image. The
// display may race in-flight writes of the newest pass by a frame;
// tears, if any, heal on the next pass.
//
// The two artifacts of the C++ original — fitting the 99-line
// 72-column listing and the sub-4096-byte binary — have no C#
// counterpart and are not reproduced here.
//
// Output: none on disk. The window closes on Escape.

using JOT.Infrastructure.Silk.Graphics;
using JOT.Infrastructure.Silk.Input;
using JOT.Infrastructure.Silk.Windowing;

// Render configuration; overridable through the command line.

var width = 480;
var height = 320;
var samplesPerSubPixel = 250;
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
    else if (args[i] == "--samples")
    {
        samplesPerSubPixel = int.Parse(args[i + 1]);
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

if (samplesPerSubPixel <= 0)
{
    throw new ArgumentException(
        $"Invalid sample count: {samplesPerSubPixel}.");
}

Console.WriteLine(
    $"Rendering {width}x{height}, {samplesPerSubPixel} samples per "
    + $"subpixel ({samplesPerSubPixel * 4} per pixel).");

// The modified Cornell box, in smallpt's original frame: radius,
// center, emission, albedo, brdf. Walls are giant spheres.

var spheres = new Sphere[]
{
    new(1e5,  new Vec3(1e5 + 1, 40.8, 81.6),   Vec3.Zero, new Vec3(.75, .25, .25),    Brdf.Diffuse),  // Left, red
    new(1e5,  new Vec3(-1e5 + 99, 40.8, 81.6), Vec3.Zero, new Vec3(.25, .25, .75),    Brdf.Diffuse),  // Right, blue
    new(1e5,  new Vec3(50, 40.8, 1e5),         Vec3.Zero, new Vec3(.75, .75, .75),    Brdf.Diffuse),  // Back
    new(1e5,  new Vec3(50, 40.8, -1e5 + 170),  Vec3.Zero, Vec3.Zero,                  Brdf.Diffuse),  // Front
    new(1e5,  new Vec3(50, 1e5, 81.6),         Vec3.Zero, new Vec3(.75, .75, .75),    Brdf.Diffuse),  // Bottom
    new(1e5,  new Vec3(50, -1e5 + 81.6, 81.6), Vec3.Zero, new Vec3(.75, .75, .75),    Brdf.Diffuse),  // Top
    new(16.5, new Vec3(27, 16.5, 47),          Vec3.Zero, new Vec3(.999, .999, .999), Brdf.Specular), // Mirror
    new(16.5, new Vec3(73, 16.5, 78),          Vec3.Zero, new Vec3(.999, .999, .999), Brdf.Glass),    // Glass
    new(600,  new Vec3(50, 681.6 - .27, 81.6), new Vec3(12, 12, 12), Vec3.Zero,       Brdf.Diffuse),  // Luminaire
};

const int lampIndex = 8;

const double Epsilon = 1e-4;

const double VacuumIndex = 1.0;

const double GlassIndex = 1.5;

// The smallpt camera: outside the room, slightly pitched down.

var cameraOrigin = new Vec3(50, 52, 295.6);

var cameraDirection = Vec3.Normalize(new Vec3(0, -0.042612, -1));

var cameraRight = new Vec3(width * .5135 / height);

var cameraUp = Vec3.Normalize(Vec3.Cross(cameraRight, cameraDirection)) * .5135;

// Nearest ray-sphere hit across the whole scene (smallpt's
// intersect(); the b+/-det candidates are evaluated in that order
// so a hairline respect for epsilon keeps surface origins from
// self-intersecting).

bool SceneHit(
    in Ray ray,
    out double distance,
    out int index)
{
    var nearest = 1e20;
    var nearestIndex = -1;

    for (var i = 0; i < spheres.Length; ++i)
    {
        var sphere = spheres[i];

        var op = sphere.Center - ray.Origin;

        var b = Vec3.Dot(op, ray.Direction);

        var discriminant = b * b
            - Vec3.Dot(op, op)
            + sphere.Radius * sphere.Radius;

        if (discriminant < 0)
        {
            continue;
        }

        var root = Math.Sqrt(discriminant);

        var candidate = b - root;

        if (candidate > Epsilon && candidate < nearest)
        {
            nearest = candidate;
            nearestIndex = i;

            continue;
        }

        candidate = b + root;

        if (candidate > Epsilon && candidate < nearest)
        {
            nearest = candidate;
            nearestIndex = i;
        }
    }

    distance = nearest;
    index = nearestIndex;

    return nearestIndex >= 0;
}

// Explicit light sampling (the first smallpt modification): cone
// sampling of the lamp's sub-tended solid angle. The contribution
// divided by the cone's solid angle concentrates direct radiance,
// which is where the soft shadows of the diffuse luminaire emerge;
// the plain bounce below suppresses lamp emission so nothing is
// double counted.

Vec3 SampleLampDirect(
    in Vec3 position,
    in Vec3 normal,
    ref ulong state)
{
    var lamp = spheres[lampIndex];

    var toLamp = lamp.Center - position;

    var distanceSquared = Vec3.Dot(toLamp, toLamp);

    var lampDirection = Vec3.Normalize(toLamp);

    // Orthonormal basis of the sampling cone.

    var axis = Math.Abs(lampDirection.X) > .1
        ? new Vec3(0, 1, 0)
        : Vec3.UnitX;

    var coneU = Vec3.Normalize(Vec3.Cross(axis, lampDirection));

    var coneV = Vec3.Cross(lampDirection, coneU);

    var cosineMax = Math.Sqrt(
        1 - lamp.Radius * lamp.Radius / distanceSquared);

    var cosine = 1 - Random(ref state) * (1 - cosineMax);

    var sine = Math.Sqrt(1 - cosine * cosine);

    var azimuth = 2 * Math.PI * Random(ref state);

    var direction = coneU * (Math.Cos(azimuth) * sine)
        + coneV * (Math.Sin(azimuth) * sine)
        + lampDirection * cosine;

    // The soft shadow test: a shadow ray through the scene; only the
    // lamp visible from the shading point contributes.

    if (SceneHit(new Ray(position, direction), out _, out var shadowIndex)
        && shadowIndex == lampIndex)
    {
        var radianceCosine = Vec3.Dot(direction, normal);

        if (radianceCosine > 0)
        {
            return spheres[lampIndex].Emission
                * (radianceCosine / (2 * Math.PI * (1 - cosineMax)));
        }
    }

    return Vec3.Zero;
}

// Path radiance in smallpt's structure, with the two published
// modifications wired in: explicit light sampling at diffuse
// vertices, and a ray tree that branches only while paths are
// shallow.

Vec3 Radiance(
    in Ray ray,
    int depth,
    bool suppressLamp,
    ref ulong state)
{
    if (!SceneHit(ray, out var distance, out var index))
    {
        return Vec3.Zero;
    }

    var sphere = spheres[index];

    var position = ray.Origin + ray.Direction * distance;

    var normal = Vec3.Normalize(position - sphere.Center);

    var forwardNormal = Vec3.Dot(normal, ray.Direction) < 0
        ? normal
        : -normal;

    var albedo = sphere.Albedo;

    if (suppressLamp && index == lampIndex)
    {
        // The explicit light sample at the previous diffuse vertex
        // accounted for this emission; counting it again would be
        // double.

        return Vec3.Zero;
    }

    ++depth;

    // Russian roulette beyond the fixed depth budget, by the largest
    // albedo channel.

    if (depth > 5)
    {
        var survival = Math.Max(albedo.X, Math.Max(albedo.Y, albedo.Z));

        if (Random(ref state) >= survival)
        {
            return sphere.Emission;
        }

        albedo /= survival;
    }

    var mirrored = ray.Direction
        - normal * (2 * Vec3.Dot(ray.Direction, normal));

    if (sphere.Brdf == Brdf.Specular)
    {
        return sphere.Emission
            + Vec3.Mult(albedo, Radiance(
                new Ray(position, mirrored),
                depth,
                false,
                ref state));
    }

    if (sphere.Brdf == Brdf.Glass)
    {
        var entering = Vec3.Dot(normal, forwardNormal) > 0;

        var indexRatio = entering
            ? VacuumIndex / GlassIndex
            : GlassIndex / VacuumIndex;

        var incidenceCosine = Vec3.Dot(ray.Direction, forwardNormal);

        var transmissionSquared = 1 - indexRatio * indexRatio
            * (1 - incidenceCosine * incidenceCosine);

        if (transmissionSquared < 0)
        {
            // Total internal reflection.

            return sphere.Emission
                + Vec3.Mult(albedo, Radiance(
                    new Ray(position, mirrored),
                    depth,
                    false,
                    ref state));
        }

        var refracted = Vec3.Normalize(
            ray.Direction * indexRatio
            - normal
                * ((entering ? 1 : -1)
                    * (incidenceCosine * indexRatio
                        + Math.Sqrt(transmissionSquared))));

        var exteriorCosine = 1 - (entering
            ? -incidenceCosine
            : Vec3.Dot(refracted, normal));

        var indexDelta = GlassIndex - VacuumIndex;
        var indexSum = GlassIndex + VacuumIndex;

        var baseReflectance = indexDelta * indexDelta / (indexSum * indexSum);

        var fresnelReflectance = baseReflectance
            + (1 - baseReflectance)
                * Power5(exteriorCosine);

        // Russian roulette and splitting for selecting reflection
        // and/or refraction (smallpt): while paths are shallow, both
        // branches are traced, weighted by their Fresnel
        // probabilities; deeper, one branch is traced through
        // Fresnel-probability Russian roulette. Branches are traced
        // lazily — only the selected one — so the ray tree stays
        // non-branching beyond the shallow stratum.

        var reflectProbability = .25 + .5 * fresnelReflectance;

        var secondary = depth > 2
            ? Random(ref state) < reflectProbability
                ? Radiance(new Ray(position, mirrored), depth, false, ref state)
                    * (fresnelReflectance / reflectProbability)
                : Radiance(new Ray(position, refracted), depth, false, ref state)
                    * ((1 - fresnelReflectance)
                        / (1 - reflectProbability))
            : Radiance(new Ray(position, mirrored), depth, false, ref state)
                    * fresnelReflectance
                + Radiance(new Ray(position, refracted), depth, false, ref state)
                    * (1 - fresnelReflectance);

        return sphere.Emission + Vec3.Mult(albedo, secondary);
    }

    // Ideal diffuse: cosine importance sampling of the hemisphere.

    var w = forwardNormal;

    var u = Vec3.Normalize(
        Vec3.Cross(
            Math.Abs(w.X) > .1 ? new Vec3(0, 1, 0) : Vec3.UnitX,
            w));

    var v = Vec3.Cross(w, u);

    var azimuth = 2 * Math.PI * Random(ref state);

    var hemisphere = Random(ref state);
    var hemisphereRoot = Math.Sqrt(hemisphere);

    var direction = Vec3.Normalize(
        u * (Math.Cos(azimuth) * hemisphereRoot)
        + v * (Math.Sin(azimuth) * hemisphereRoot)
        + w * Math.Sqrt(1 - hemisphere));

    // Explicit light sampling of the luminaire.

    var direct = SampleLampDirect(position, forwardNormal, ref state);

    // One non-branching indirect bounce; lamp emission is suppressed
    // along it because the solid-angle sample above accounted for it.

    var indirect = Radiance(
        new Ray(position, direction),
        depth,
        true,
        ref state);

    return sphere.Emission
        + Vec3.Mult(albedo, direct + indirect);
}

// Schlick's fifth-power falloff, kept named for readability in the
// glass branch above.

double Power5(
    double value)
{
    var squared = value * value;

    return squared * squared * value;
}

// xoshiro-style 64-bit shift register with a per-thread state, the
// counterpart of smallpt's per-thread erand48 state. Returns uniform
// [0, 1).

double Random(ref ulong state)
{
    state ^= state >> 12;
    state ^= state << 25;
    state ^= state >> 27;

    var mixed = state * 2685821657736338717UL;

    return (mixed >> 11) * (1.0 / 9007199254740992.0);
}

// Progressive accumulation: one Vec3 radiance sum per stratum (four
// strata per pixel, rows bottom to top).

var accumulator = new Vec3[width * height * 4];

var passCount = 0;

var passTarget = samplesPerSubPixel;

var renderCompleted = new ManualResetEventSlim(false);

var stopwatch = System.Diagnostics.Stopwatch.StartNew();

// The render task runs alongside the window loop, advancing the
// accumulator one full pass (one radiance sample per stratum) at a
// time, striped over all logical cores through dynamically claimed
// columns — the C# counterpart of smallpt's OpenMP row loop. The
// tracing runs on dedicated threads with generous stacks: Russian
// roulette chains through the mirror and glass surfaces are
// unbounded by design (smallpt relies on the same), and pooled
// threads carry only small default stacks. A cyclic barrier pairs
// every pass release with its completion so pass boundaries are
// exact.

var threadCount = Environment.ProcessorCount;

var activePass = 0;

var nextColumn = -1L;

var passBarrier = new Barrier(threadCount + 1);

for (var threadIndex = 0; threadIndex < threadCount; ++threadIndex)
{
    var workerIndex = threadIndex;

    var worker = new Thread(
        () =>
        {
            // One long-lived state per thread, shaped by the pass so
            // the sample streams stay uncorrelated across passes.

            var state = 0x9E3779B97F4A7C15UL
                ^ (ulong)(uint)(workerIndex * 1013904223u + 40503u);

            for (;;)
            {
                passBarrier.SignalAndWait();

                if (renderCompleted.IsSet)
                {
                    return;
                }

                var pass = Volatile.Read(ref activePass);

                long claimed;

                while ((claimed = Interlocked.Increment(ref nextColumn)) < width)
                {
                    var column = (int)claimed;

                    // smallpt rasterizes rows bottom to top; the
                    // camera formula expects that row number.

                    for (var row = 0; row < height; ++row)
                    {
                        var pixel = row * width + column;

                        var smallY = height - 1 - row;

                        var stratum = pixel * 4;

                        for (var subY = 0; subY < 2; ++subY)
                        {
                            for (var subX = 0; subX < 2; ++subX)
                            {
                                accumulator[stratum + subY * 2 + subX]
                                    += Radiance(
                                        SubPixelRay(
                                            column,
                                            smallY,
                                            subX,
                                            subY,
                                            ref state),
                                        0,
                                        false,
                                        ref state);
                            }
                        }
                    }
                }
            }
        },
        64 * 1024 * 1024)
    {
        IsBackground = true,
        Name = $"JOT-RayDemo-Tracer-{threadIndex}",
    };

    worker.Start();
}

// One orchestrator phase-pairs the passes: a first SignalAndWait
// releases the workers into a pass, a second waits for its
// completion. When the pass budget is reached — or the window closed
// early — the workers observe the completion flag at the next
// release and exit; being background threads they never block the
// process on the way out.

ThreadPool.QueueUserWorkItem(
    _ =>
    {
        try
        {
            for (var pass = 0; pass < passTarget && !renderCompleted.IsSet; ++pass)
            {
                passBarrier.SignalAndWait();

                passBarrier.SignalAndWait();

                passCount = pass + 1;

                if (passCount == 1 || passCount % 25 == 0)
                {
                    Console.WriteLine(
                        $"  pass {passCount}/{passTarget} "
                        + $"({passCount * 100 / passTarget}%, "
                        + $"{stopwatch.Elapsed.TotalSeconds:F1}s).");
                }

                Volatile.Write(ref activePass, pass + 1);

                nextColumn = -1;
            }
        }
        finally
        {
            renderCompleted.Set();
        }
    });

// Camera ray through one stratum of one pixel, jittered by the
// importance-sampled tent distribution (smallpt's camera loop; the
// origin is pushed forward so rays start inside the interior).

Ray SubPixelRay(
    int pixelX,
    int smallY,
    int subX,
    int subY,
    ref ulong state)
{
    // Tent: fold two uniforms into a triangular distribution.

    var r1 = 2 * Random(ref state);
    var r2 = 2 * Random(ref state);

    var tentX = r1 < 1 ? Math.Sqrt(r1) - 1 : 1 - Math.Sqrt(2 - r1);
    var tentY = r2 < 1 ? Math.Sqrt(r2) - 1 : 1 - Math.Sqrt(2 - r2);

    var filmX = ((subX + .5 + tentX) / 2 + pixelX) / width - .5;
    var filmY = ((subY + .5 + tentY) / 2 + smallY) / height - .5;

    var direction = cameraRight * filmX
        + cameraUp * filmY
        + cameraDirection;

    return new Ray(
        cameraOrigin + direction * 140,
        Vec3.Normalize(direction));
}

// Display: a fullscreen quad textured with the current mean of the
// accumulator, tone mapped exactly like smallpt's toInt (clamp,
// gamma 2.2). The accumulator rows run bottom to top and GL texture
// uploads interpret the first row as v = 0, the quad's bottom edge,
// so rows map straight.

var window = new SilkWindow(
    "JOT.NET RayDemo — smallpt Cornell box",
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

var uploadedPass = -1;

var frameCount = 0;

// Tone-maps the current pass mean into RGBA pixels. Each stratum's
// sample sum divides by the pass count before the smallpt weight of
// one quarter per stratum is applied.

void BakeDisplayPixels(
    byte[] pixels,
    int passes)
{
    var stratumMean = .25 / passes;

    for (var pixel = 0; pixel < width * height; ++pixel)
    {
        Vec3 mean = default;

        for (var sub = 0; sub < 4; ++sub)
        {
            mean += accumulator[pixel * 4 + sub] * stratumMean;
        }

        BakeChannel(pixels, pixel, mean.X, 0);
        BakeChannel(pixels, pixel, mean.Y, 1);
        BakeChannel(pixels, pixel, mean.Z, 2);

        pixels[pixel * 4 + 3] = byte.MaxValue;
    }
}

void BakeChannel(
    byte[] pixels,
    int pixel,
    double value,
    int channel)
{
    var clamped = Math.Clamp(value, 0.0, 1.0);

    var gamma = Math.Pow(clamped, 1.0 / 2.2);

    pixels[pixel * 4 + channel] = (byte)Math.Round(gamma * byte.MaxValue);
}

window.Run((currentWindow, deltaTime) =>
{
    if (displayProgram is null)
    {
        displayProgram = currentWindow.Graphics.CreateShaderProgram(
            DisplayVertexShaderSource,
            DisplayFragmentShaderSource);

        // Fullscreen quad on the fixed vertex layout (position,
        // color, normal, uv). Uv v spans 0 at the bottom edge so the
        // texture's first row (the bottom of the image) draws there.

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

    // Refresh the display texture whenever a new pass landed.

    var currentPass = Volatile.Read(ref passCount);

    if (currentPass != uploadedPass && currentPass > 0)
    {
        var pixels = new byte[width * height * 4];

        BakeDisplayPixels(pixels, currentPass);

        displayTexture?.Dispose();

        displayTexture = currentWindow.Graphics.CreateTexture(
            width,
            height,
            pixels);

        uploadedPass = currentPass;
    }

    currentWindow.Graphics.Clear(0.0f, 0.0f, 0.0f, 1.0f);

    if (displayTexture is not null)
    {
        displayProgram!.Use();
        displayProgram.SetUniform("uTexture", 0);

        displayTexture.Bind(0);

        currentWindow.Graphics.DrawTriangles(quadBuffer!);
    }

    if (currentWindow.Input.Keyboard.IsKeyPressed(KeyCode.Escape)
        || (frameLimit >= 0 && ++frameCount >= frameLimit)
        || (secondsLimit >= 0 && stopwatch.Elapsed.TotalSeconds >= secondsLimit)
        || Volatile.Read(ref passCount) >= passTarget)
    {
        Console.WriteLine(
            $"Closed at pass {Volatile.Read(ref passCount)}/{passTarget}.");

        renderCompleted.Set();

        currentWindow.Close();
    }
});

// One Cornell box participant: a sphere with emission, albedo and a
// BRDF class. Walls are spheres of radius 1e5, as in smallpt.

internal readonly record struct Sphere(
    double Radius,
    Vec3 Center,
    Vec3 Emission,
    Vec3 Albedo,
    Brdf Brdf);

internal enum Brdf
{
    Diffuse,
    Specular,
    Glass,
}

internal readonly record struct Ray(
    Vec3 Origin,
    Vec3 Direction);

// Double-precision 3-vector with the operator surface the tracer
// needs; System.Numerics is single precision, too weak for
// double-precision path tracing accumulation math.

internal readonly record struct Vec3(
    double X,
    double Y,
    double Z)
{
    // smallpt's Vec(x_) fills the remaining channels with zero.

    public Vec3(double x)
        : this(x, 0, 0)
    {
    }

    public static Vec3 UnitX => new(1, 0, 0);

    public static Vec3 Zero => default;

    public static Vec3 operator +(Vec3 left, Vec3 right)
    {
        return new Vec3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static Vec3 operator -(Vec3 left, Vec3 right)
    {
        return new Vec3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    public static Vec3 operator -(Vec3 value)
    {
        return new Vec3(-value.X, -value.Y, -value.Z);
    }

    public static Vec3 operator *(Vec3 left, double scalar)
    {
        return new Vec3(left.X * scalar, left.Y * scalar, left.Z * scalar);
    }

    public static Vec3 operator *(double scalar, Vec3 right)
    {
        return right * scalar;
    }

    public static Vec3 operator /(Vec3 left, double scalar)
    {
        return new Vec3(left.X / scalar, left.Y / scalar, left.Z / scalar);
    }

    public static double Dot(Vec3 left, Vec3 right)
    {
        return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
    }

    public static Vec3 Cross(Vec3 left, Vec3 right)
    {
        return new Vec3(
            left.Y * right.Z - left.Z * right.Y,
            left.Z * right.X - left.X * right.Z,
            left.X * right.Y - left.Y * right.X);
    }

    public static Vec3 Normalize(Vec3 value)
    {
        return value / Math.Sqrt(Dot(value, value));
    }

    // Component-wise product (smallpt's mult), used for the albedo
    // absorption of secondary radiance.

    public static Vec3 Mult(Vec3 left, Vec3 right)
    {
        return new Vec3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
    }
}
