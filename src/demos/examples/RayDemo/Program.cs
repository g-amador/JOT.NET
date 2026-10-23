// JOT.NET RayDemo: a smallpt-style CPU Monte Carlo path tracer
// (after Kevin Beason's 99-line smallpt, 2008).
//
// Scene: a small closed room holding a glass sphere, a mirror cube and
// a diffuse cone, lit by an emissive sphere lamp near the ceiling. The
// tracer implements the smallpt BRDF set: ideal diffuse (cosine-weighted
// hemisphere importance sampling), ideal specular reflection and ideal
// dielectric glass with Fresnel reflect/refract probability splitting.
// Soft color bleeding and soft shadows emerge from Monte Carlo sampling;
// path depth is controlled by Russian roulette.
//
// Core reuse: JOT.Core.Math supplies the Ray primitive and the six room
// wall plane intersections (RayIntersections: boolean test plus
// distance). The remaining closest-hit queries (sphere, box, capped
// cone) and the BRDF machinery stand in this demo, because the
// Core.Math intersection helpers expose boolean tests only, which
// cannot drive a renderer.
//
// Output: a 24-bit BMP of the rendered frame.

using System.Numerics;
using JOT.Core.Math.Geometry;
using JOT.Core.Math.Geometry.Intersections;
using Plane = JOT.Core.Math.Geometry.Plane;

// Render configuration; overridable through the command line.

var width = 480;
var height = 320;
var samplesPerPixel = 64;
var outputPath = "raydemo.bmp";

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
        samplesPerPixel = int.Parse(args[i + 1]);
    }
    else if (args[i] == "--output")
    {
        outputPath = args[i + 1];
    }
}

if (width <= 0 || height <= 0)
{
    throw new ArgumentException(
        $"Invalid render resolution: {width}x{height}.");
}

if (samplesPerPixel <= 0)
{
    throw new ArgumentException(
        $"Invalid sample count: {samplesPerPixel}.");
}

// The room: an axis-aligned interior, x in [-3, 3], y in [0, 6],
// z in [-4, 2], closed by six inward-facing planes. The wall planes
// reuse the Core.Math intersection helper; the Cornell-style tinted
// side walls demonstrate diffuse color bleeding.

var walls = new (Plane Plane, Vector3 Reflectance)[]
{
    (new Plane(new Vector3(1.0f, 0.0f, 0.0f), 3.0f),
        new Vector3(0.75f, 0.25f, 0.25f)), // left, red
    (new Plane(new Vector3(-1.0f, 0.0f, 0.0f), 3.0f),
        new Vector3(0.25f, 0.25f, 0.75f)), // right, blue
    (new Plane(new Vector3(0.0f, 1.0f, 0.0f), 0.0f),
        new Vector3(0.75f, 0.75f, 0.75f)), // floor
    (new Plane(new Vector3(0.0f, -1.0f, 0.0f), 6.0f),
        new Vector3(0.75f, 0.75f, 0.75f)), // ceiling
    (new Plane(new Vector3(0.0f, 0.0f, 1.0f), 4.0f),
        new Vector3(0.75f, 0.75f, 0.75f)), // back
    (new Plane(new Vector3(0.0f, 0.0f, -1.0f), 2.0f),
        new Vector3(0.75f, 0.75f, 0.75f)), // front, behind the camera
};

// Scene object parameters.

var glassSphereCenter = new Vector3(1.8f, 0.9f, -1.5f);
var glassSphereRadius = 0.9f;
var glassReflectance = new Vector3(0.999f, 0.999f, 0.999f);

var cubeMinimum = new Vector3(-2.55f, 0.0f, -3.75f);
var cubeMaximum = new Vector3(-1.45f, 2.0f, -2.65f);
var cubeReflectance = new Vector3(0.99f, 0.99f, 0.99f);

var coneApex = new Vector3(-0.2f, 3.0f, -2.4f);
var coneAxis = new Vector3(0.0f, -1.0f, 0.0f); // apex toward the base
var coneHeight = 3.0f;
var coneBaseRadius = 1.0f;
var coneReflectance = new Vector3(0.75f, 0.55f, 0.35f);

// Emissive lamp sphere floating under the ceiling: a black diffuse
// surface radiating white. Every indirect path through the room's
// interior sampling produces the soft shadows and soft highlights.

var lampCenter = new Vector3(0.0f, 5.45f, -1.5f);
var lampRadius = 1.4f;
var lampEmission = new Vector3(10.0f, 8.7f, 7.4f);

bool TryHitScene(
    in Ray ray,
    out Hit hit)
{
    const float epsilon = 0.0015f;

    var best = float.MaxValue;
    var bestHit = default(Hit?);

    void TryAccept(Hit? candidate)
    {
        if (candidate is { } found && found.Distance > epsilon
            && found.Distance < best)
        {
            best = found.Distance;
            bestHit = found;
        }
    }

    foreach (var (plane, reflectance) in walls)
    {
        if (RayIntersections.Intersects(ray, plane, out var distance))
        {
            TryAccept(new Hit(
                distance,
                Face(ray, plane.Normal),
                reflectance,
                Vector3.Zero,
                Brdf.Diffuse));
        }
    }

    TryAccept(HitSphere(ray, glassSphereCenter, glassSphereRadius, glassReflectance, Brdf.Glass));

    TryAccept(HitBox(ray, cubeMinimum, cubeMaximum, cubeReflectance, Brdf.Specular));

    TryAccept(HitCone(ray, coneApex, coneAxis, coneHeight, coneBaseRadius, coneReflectance));

    TryAccept(HitSphere(ray, lampCenter, lampRadius, Vector3.Zero, Brdf.Diffuse, lampEmission));

    hit = bestHit ?? default;

    return bestHit is not null;
}

// Flips a surface normal against the incident ray like smallpt does, so
// shading only ever sees a front-facing surface.

Vector3 Face(
    in Ray ray,
    Vector3 normal)
{
    return Vector3.Dot(normal, ray.Direction) > 0.0f ? -normal : normal;
}

// Closest ray-sphere hit: the usual quadratic.

Hit? HitSphere(
    in Ray ray,
    Vector3 center,
    float radius,
    Vector3 reflectance,
    Brdf brdf,
    Vector3 emission = default)
{
    var offset = ray.Origin - center;

    var b = 2.0f * Vector3.Dot(offset, ray.Direction);
    var c = Vector3.Dot(offset, offset) - radius * radius;

    var discriminant = b * b - 4.0f * c;

    if (discriminant < 0.0f)
    {
        return null;
    }

    var root = MathF.Sqrt(discriminant);

    var near = (-b - root) / 2.0f;
    var far = (-b + root) / 2.0f;

    var distance = near > 0.0015f ? near : far;

    if (distance <= 0.0015f)
    {
        // No positive-time hit (ray behind the sphere, grazing start).

        return null;
    }

    var normal = Vector3.Normalize(
        ray.Origin + ray.Direction * distance - center);

    return new Hit(distance, normal, reflectance, emission, brdf);
}

// Closest ray-axis-aligned-box hit by slab testing; the entry axis and
// its sign give the face normal.

Hit? HitBox(
    in Ray ray,
    Vector3 minimum,
    Vector3 maximum,
    Vector3 reflectance,
    Brdf brdf,
    Vector3 emission = default)
{
    const float epsilon = 0.0015f;

    Span<float> origins = [ray.Origin.X, ray.Origin.Y, ray.Origin.Z];
    Span<float> directions = [ray.Direction.X, ray.Direction.Y, ray.Direction.Z];
    Span<float> minimums = [minimum.X, minimum.Y, minimum.Z];
    Span<float> maximums = [maximum.X, maximum.Y, maximum.Z];

    var near = 0.0f;
    var far = float.MaxValue;
    var nearAxis = -1;
    var nearSign = false;

    for (var axis = 0; axis < 3; ++axis)
    {
        if (MathF.Abs(directions[axis]) < 1e-8f)
        {
            if (origins[axis] < minimums[axis] || origins[axis] > maximums[axis])
            {
                return null;
            }

            continue;
        }

        var inverse = 1.0f / directions[axis];
        var fromMinimumFace = (minimums[axis] - origins[axis]) * inverse;
        var fromMaximumFace = (maximums[axis] - origins[axis]) * inverse;

        // Entering through the maximum face means the face normal
        // points along the positive axis; through the minimum face,
        // along the negative axis.

        var t0 = fromMinimumFace;
        var entersMaximumFace = false;

        if (t0 > fromMaximumFace)
        {
            t0 = fromMaximumFace;
            entersMaximumFace = true;
        }

        var t1 = MathF.Max(fromMinimumFace, fromMaximumFace);

        if (t0 > near)
        {
            near = t0;
            nearAxis = axis;
            nearSign = entersMaximumFace;
        }

        far = MathF.Min(far, t1);

        if (near > far)
        {
            return null;
        }
    }

    var start = near > epsilon ? near : far;

    if (start <= epsilon || nearAxis < 0)
    {
        return null;
    }

    var normal = nearAxis switch
    {
        0 => new Vector3(nearSign ? 1.0f : -1.0f, 0.0f, 0.0f),
        1 => new Vector3(0.0f, nearSign ? 1.0f : -1.0f, 0.0f),
        _ => new Vector3(0.0f, 0.0f, nearSign ? 1.0f : -1.0f),
    };

    return new Hit(start, normal, reflectance, emission, brdf);
}

// Closest ray-capped-cone hit: a quadratic over the implicit cone body
// (axis running apex toward the base, half-angle implied by height and
// base radius), filtered to the axial span, plus the closing base disc.

Hit? HitCone(
    in Ray ray,
    Vector3 apex,
    Vector3 axis,
    float height,
    float baseRadius,
    Vector3 reflectance)
{
    const float epsilon = 0.0015f;

    var axisUnit = Vector3.Normalize(axis);

    // cos^2 of the cone half-angle.

    var cosineSquared = height * height
        / (height * height + baseRadius * baseRadius);

    var offset = ray.Origin - apex;

    var m = Vector3.Dot(offset, axisUnit);
    var n = Vector3.Dot(ray.Direction, axisUnit);

    var quadraticA = n * n
        - cosineSquared * Vector3.Dot(ray.Direction, ray.Direction);
    var quadraticB = 2.0f
        * (n * m - cosineSquared * Vector3.Dot(offset, ray.Direction));
    var quadraticC = m * m
        - cosineSquared * Vector3.Dot(offset, offset);

    var best = float.MaxValue;
    var normal = Vector3.Zero;

    if (MathF.Abs(quadraticA) > 1e-8f)
    {
        var discriminant = quadraticB * quadraticB
            - 4.0f * quadraticA * quadraticC;

        if (discriminant >= 0.0f)
        {
            var root = MathF.Sqrt(discriminant);
            var inverse = 1.0f / (2.0f * quadraticA);
            var d0 = (-quadraticB - root) * inverse;
            var d1 = (-quadraticB + root) * inverse;

            foreach (var distance in (float[])[d0, d1])
            {
                if (distance < epsilon || distance >= best)
                {
                    continue;
                }

                var surfaceOffset = ray.Origin
                    + ray.Direction * distance
                    - apex;

                var axialHeight = Vector3.Dot(surfaceOffset, axisUnit);

                if (axialHeight < 0.0f || axialHeight > height)
                {
                    continue;
                }

                // Implicit-surface gradient gives the cone normal.

                normal = Vector3.Normalize(
                    cosineSquared * surfaceOffset - axialHeight * axisUnit);

                best = distance;
            }
        }
    }

    // Base disc closing the cone, outward normal opposite the axis.

    var baseCenter = apex + axisUnit * height;
    var baseNormal = -axisUnit;

    var denominator = Vector3.Dot(baseNormal, ray.Direction);

    if (MathF.Abs(denominator) > 1e-8f)
    {
        var distance = Vector3.Dot(baseNormal, baseCenter - ray.Origin)
            / denominator;

        if (distance > epsilon && distance < best)
        {
            var point = ray.Origin + ray.Direction * distance;

            if (Vector3.DistanceSquared(point, baseCenter) <= baseRadius * baseRadius)
            {
                normal = baseNormal;
                best = distance;
            }
        }
    }

    if (best >= float.MaxValue)
    {
        return null;
    }

    return new Hit(best, normal, reflectance, Vector3.Zero, Brdf.Diffuse);
}

float Random(ref ulong state)
{
    state ^= state >> 12;
    state ^= state << 25;
    state ^= state >> 27;

    var value = state * 2685821657736338717UL;

    return (value >> 11) * (2.2204460492503130808472633361816e-16f);
}

// Path radiance in smallpt's structure: trace, then shade by BRDF class
// with Russian roulette deeper than the fixed depth budget. Recursion is
// bounded: every branch increments the depth and most paths terminate
// through the roulette term.

Vector3 Radiance(
    in Ray ray,
    int depth,
    ref ulong state)
{
    if (!TryHitScene(ray, out var hit))
    {
        return Vector3.Zero;
    }

    var forwardNormal = Vector3.Dot(hit.Normal, ray.Direction) < 0.0f
        ? hit.Normal
        : -hit.Normal;

    var reflectance = hit.Reflectance;

    // Russian roulette probability: the largest reflectance channel.

    var survival = MathF.Max(
        reflectance.X,
        MathF.Max(reflectance.Y, reflectance.Z));

    ++depth;

    if (depth > 5)
    {
        if (Random(ref state) >= survival)
        {
            return hit.Emission;
        }

        reflectance /= survival;
    }

    // Secondary ray origins sit on the surface; bias toward the traced
    // hemisphere to avoid re-hitting the same surface.

    var reflectedOrigin = hit.Position(ray) + forwardNormal * 0.002f;

    if (hit.Brdf == Brdf.Diffuse)
    {
        // Cosine-weighted importance sampling over the hemisphere: the
        // pdf is cos(theta)/pi, matching the Lambert BRDF factor, so no
        // cosine division appears here.

        var azimuth = 2.0f * MathF.PI * Random(ref state);

        var vertical = Random(ref state);
        var verticalRoot = MathF.Sqrt(vertical);

        var w = forwardNormal;

        var u = Vector3.Normalize(Vector3.Cross(
            MathF.Abs(w.X) > 0.1f
                ? new Vector3(0.0f, 1.0f, 0.0f)
                : Vector3.UnitX,
            w));

        var v = Vector3.Cross(w, u);

        var direction = Vector3.Normalize(
            u * MathF.Cos(azimuth) * verticalRoot
            + v * MathF.Sin(azimuth) * verticalRoot
            + w * MathF.Sqrt(1.0f - vertical));

        var incoming = Radiance(new Ray(reflectedOrigin, direction), depth, ref state);

        return hit.Emission + incoming * reflectance;
    }

    var mirrored = Vector3.Reflect(ray.Direction, hit.Normal);

    if (hit.Brdf == Brdf.Specular)
    {
        var incoming = Radiance(new Ray(reflectedOrigin, mirrored), depth, ref state);

        return hit.Emission + incoming * reflectance;
    }

    // Ideal dielectric glass: split reflection and refraction with the
    // Fresnel probability (Schlick's approximation), as smallpt does.

    var into = Vector3.Dot(hit.Normal, forwardNormal) > 0.0f;

    const float vacuumIndex = 1.0f;
    const float glassIndex = 1.5f;

    var indexRatio = into
        ? vacuumIndex / glassIndex
        : glassIndex / vacuumIndex;

    var incidenceCosine = Vector3.Dot(ray.Direction, forwardNormal);

    var refractionCosineSquared = 1.0f
        - indexRatio * indexRatio
        * (1.0f - incidenceCosine * incidenceCosine);

    if (refractionCosineSquared < 0.0f)
    {
        // Total internal reflection.

        var incoming = Radiance(new Ray(reflectedOrigin, mirrored), depth, ref state);

        return hit.Emission + incoming * reflectance;
    }

    // Secondary ray origin for the transmitted branch points into the
    // sphere.

    var refractedOrigin = hit.Position(ray) - forwardNormal * 0.002f;

    var transmitted = ray.Direction * indexRatio
        - hit.Normal
            * ((into ? 1.0f : -1.0f)
                * (incidenceCosine * indexRatio
                    + MathF.Sqrt(refractionCosineSquared)));

    var exteriorCosine = into
        ? -incidenceCosine
        : Vector3.Dot(
            Vector3.Normalize(transmitted),
            hit.Normal);

    var indexDelta = glassIndex - vacuumIndex;
    var indexSum = glassIndex + vacuumIndex;

    var baseReflectance = indexDelta * indexDelta / (indexSum * indexSum);

    var falloff = 1.0f - exteriorCosine;

    var fresnelReflectance = baseReflectance
        + (1.0f - baseReflectance)
        * falloff * falloff * falloff * falloff * falloff;

    var reflectProbability = 0.25f + 0.5f * fresnelReflectance;

    if (Random(ref state) < reflectProbability)
    {
        var incoming = Radiance(new Ray(reflectedOrigin, mirrored), depth, ref state);

        return hit.Emission
            + incoming * reflectance
                * (fresnelReflectance / reflectProbability);
    }

    var transmittedIncoming = Radiance(
        new Ray(refractedOrigin, Vector3.Normalize(transmitted)),
        depth,
        ref state);

    return hit.Emission
        + transmittedIncoming * reflectance
            * ((1.0f - fresnelReflectance) / (1.0f - reflectProbability));
}

// Pinhole camera: slight downward pitch into the room, a ~58 degree
// vertical field of view and a half-pixel jitter per sample.

var forward = Vector3.Normalize(new Vector3(0.0f, -0.18f, -1.0f));
var cameraRight = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
var cameraUp = Vector3.Normalize(Vector3.Cross(cameraRight, forward));

var cameraOrigin = new Vector3(0.0f, 2.35f, 1.7f);

var filmScale = 0.55f;
var filmScaleX = filmScale * width / height;

var pixels = new Vector3[width * height];

var randomState = 0x9E3779B97F4A7C15UL;

Console.WriteLine(
    $"Rendering {width}x{height} at {samplesPerPixel} samples per pixel.");

var rowCountDone = 0;

for (var y = 0; y < height; ++y)
{
    for (var x = 0; x < width; ++x)
    {
        var color = Vector3.Zero;

        for (var s = 0; s < samplesPerPixel; ++s)
        {
            var jitterX = Random(ref randomState) - 0.5f;
            var jitterY = Random(ref randomState) - 0.5f;

            var filmX = (2.0f * (x + jitterX) / width - 1.0f) * filmScaleX;
            var filmY = (1.0f - 2.0f * (y + jitterY) / height) * filmScale;

            var direction = Vector3.Normalize(
                forward + cameraRight * filmX + cameraUp * filmY);

            color += Radiance(
                new Ray(cameraOrigin, direction),
                0,
                ref randomState);
        }

        pixels[y * width + x] = color / samplesPerPixel;
    }

    if (++rowCountDone % 20 == 0)
    {
        Console.WriteLine(
            $"  row {rowCountDone}/{height} ({rowCountDone * 100 / height}%).");
    }
}

// Tone mapping: clamp and gamma 2.2, as smallpt's toInt().

var bitmapBytes = new byte[width * height * 3];

for (var i = 0; i < width * height; ++i)
{
    var color = pixels[i];

    static byte ToChannel(float channel)
    {
        var gamma = MathF.Pow(
            Math.Clamp(channel, 0.0f, 1.0f),
            1.0f / 2.2f);

        return (byte)MathF.Round(gamma * 255.0f);
    }

    bitmapBytes[i * 3 + 0] = ToChannel(color.X);
    bitmapBytes[i * 3 + 1] = ToChannel(color.Y);
    bitmapBytes[i * 3 + 2] = ToChannel(color.Z);
}

// Write the 24-bit BMP. BMP rows run bottom to top; the pixel array is
// top down, so the rows write in reverse order with BGR pixels.

var rowBytes = width * 3;
var pad = (4 - rowBytes % 4) % 4;
var dataSize = (rowBytes + pad) * height;

using (var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
{
    using var writer = new BinaryWriter(stream);

    writer.Write((byte)0x42);
    writer.Write((byte)0x4D);
    writer.Write(54 + dataSize);
    writer.Write((short)0);
    writer.Write((short)0);
    writer.Write(54);
    writer.Write(40);
    writer.Write(width);
    writer.Write(height);
    writer.Write((short)1);
    writer.Write((short)24);
    writer.Write(0);
    writer.Write(dataSize);
    writer.Write(0);
    writer.Write(0);
    writer.Write(0);
    writer.Write(0);

    for (var y = height - 1; y >= 0; --y)
    {
        var rowOffset = y * width * 3;

        for (var x = 0; x < width; ++x)
        {
            writer.Write(bitmapBytes[rowOffset + x * 3 + 2]);
            writer.Write(bitmapBytes[rowOffset + x * 3 + 1]);
            writer.Write(bitmapBytes[rowOffset + x * 3 + 0]);
        }

        for (var p = 0; p < pad; ++p)
        {
            writer.Write((byte)0);
        }
    }
}

Console.WriteLine($"Wrote '{outputPath}'.");

// Renderable scene intersection record: the distance chooses the
// closest hit; the normal, reflectance and emission drive the BRDF
// shading; the BRDF class selects the scatter behavior.

enum Brdf
{
    Diffuse,
    Specular,
    Glass,
}

readonly struct Hit
{
    public Hit(
        float distance,
        Vector3 normal,
        Vector3 reflectance,
        Vector3 emission,
        Brdf brdf)
    {
        Distance = distance;
        Normal = normal;
        Reflectance = reflectance;
        Emission = emission;
        Brdf = brdf;
    }

    public float Distance { get; }

    public Vector3 Normal { get; }

    public Vector3 Reflectance { get; }

    public Vector3 Emission { get; }

    public Brdf Brdf { get; }

    // World-space hit point along the traced ray.

    public Vector3 Position(
        in Ray ray)
    {
        return ray.Origin + ray.Direction * Distance;
    }
}
