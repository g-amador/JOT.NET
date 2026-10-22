// Showcase demo for the Core audio pipeline, kept permanently:
// device -> listener -> positional sound emitter.
// Menu-driven with number keys: 1 procedural sine wave, 2 real .wav
// asset, 3 Super Mario theme, 4 Tetris theme, 5 Zelda (Song of Storms). Everything plays through
// the engine (no Console.Beep): melodies are synthesized from note
// tables transcribed from the original beep implementation and rendered
// as engine samples. Press any key to stop playback early; Q/Esc quits.
// With redirected input (scripted runs) choices are read line by line
// from stdin, and playback runs to completion.

using JOT.Core.Audio;
using JOT.Infrastructure.Silk.Audio;
using System.Numerics;

const int SampleRate = 44100;

using var device = new SilkAudioDevice();

// Listener at the world origin, facing down the forward axis.
var listener = new Listener(device);
listener.Synchronize();

while (true)
{
    Console.WriteLine();
    Console.WriteLine("JOT.NET audio smoke");
    Console.WriteLine("  1 - Procedural sine wave (engine)");
    Console.WriteLine("  2 - WAV file (engine): Weapons/pdp1_fire.wav");
    Console.WriteLine("  3 - Super Mario theme (engine)");
    Console.WriteLine("  4 - Tetris theme (engine)");
    Console.WriteLine("  5 - Zelda: Song of Storms (engine)");
    Console.WriteLine("  Q - Quit");
    Console.Write("Choice: ");

    char choice;

    if (Console.IsInputRedirected)
    {
        var line = Console.ReadLine();

        if (line is null)
        {
            break;
        }

        choice = line.Trim().ToLowerInvariant() is { Length: > 0 } trimmed
            ? trimmed[0]
            : ' ';
    }
    else
    {
        choice = char.ToLowerInvariant(Console.ReadKey(intercept: true).KeyChar);
        Console.WriteLine();
    }

    if (choice is 'q' or '\u001b')
    {
        break;
    }

    switch (choice)
    {
        case '1':
            Play(device, SineSamples(SampleRate), SampleRate, 1, "Sine wave");
            break;

        case '2':
            var wave = WaveFile.Load(FindSoundPath());
            Console.WriteLine(
                $"Loaded: {wave.SampleRate} Hz, {wave.Channels} channel(s), "
                + $"{wave.Seconds:0.00} s.");
            Play(device, wave.Samples, wave.SampleRate, wave.Channels, "WAV file");
            break;

        case '3':
            Play(device,
                SynthesizeMelody(MarioTheme(), SampleRate),
                SampleRate,
                1,
                "Super Mario theme");
            break;

        case '4':
            Play(device,
                SynthesizeMelody(TetrisTheme(), SampleRate),
                SampleRate,
                1,
                "Tetris theme");
            break;

        case '5':
            Play(device,
                SynthesizeMelody(StormsTheme(), SampleRate),
                SampleRate,
                1,
                "Zelda: Song of Storms");
            break;
    }
}

Console.WriteLine("Audio smoke closed normally.");

static void Play(
    IAudioDevice device,
    short[] samples,
    int sampleRate,
    int channels,
    string label)
{
    Console.WriteLine($"{label}: playing - press any key to stop...");

    // Place the emitter one unit to the right of the listener: positioned
    // audio should then be perceived on the right side.
    var emitter = new SoundEmitter(device, samples, sampleRate, channels)
    {
        Position = new Vector3(1.0f, 0.0f, 0.0f),
    };

    emitter.Play();

    var requestedStop = false;

    while (emitter.IsPlaying)
    {
        if (!Console.IsInputRedirected && Console.KeyAvailable)
        {
            Console.ReadKey(intercept: true);
            requestedStop = true;

            break;
        }

        Thread.Sleep(50);
    }

    if (requestedStop)
    {
        emitter.Stop();
        Console.WriteLine($"{label}: stopped early.");
    }
    else
    {
        Console.WriteLine($"{label}: finished.");
    }
}

// Generates one second of a 440 Hz sine wave, mono, signed 16-bit.
static short[] SineSamples(int sampleRate)
{
    const int Frequency = 440;

    var samples = new short[sampleRate];

    for (var i = 0; i < samples.Length; i++)
    {
        samples[i] = (short)
            (MathF.Sin(i * 2.0f * MathF.PI * Frequency / sampleRate)
                * (short.MaxValue * 0.6f));
    }

    return samples;
}

// Locates the repository assets folder by walking up from the build
// output (bin/Debug/net10.0), so the harness runs from any working
// directory.
static string FindSoundPath()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null)
    {
        var candidate = Path.Combine(
            directory.FullName,
            "assets",
            "sounds",
            "Weapons",
            "pdp1_fire.wav");

        if (File.Exists(candidate))
        {
            return candidate;
        }

        directory = directory.Parent;
    }

    throw new InvalidOperationException(
        "Could not find the assets/sounds folder relative to the build output.");
}

// Renders (frequency, milliseconds) note tables as mono signed 16-bit
// PCM through the engine. Frequency zero means silence (a rest).
// A short fade on both ends of each note removes the click the beep
// version produces between notes; notes and durations are unchanged.
static short[] SynthesizeMelody(
    (int Frequency, int Milliseconds)[] notes,
    int sampleRate)
{
    var totalMilliseconds = 0;

    foreach (var note in notes)
    {
        totalMilliseconds += note.Milliseconds;
    }

    var samples = new short[(long)totalMilliseconds * sampleRate / 1000];

    const double amplitude = 0.45 * short.MaxValue;
    const int FadeMilliseconds = 4;
    var fade = Math.Max(1, FadeMilliseconds * sampleRate / 1000);

    var cursor = 0;
    var phaseIndex = 0;

    foreach (var note in notes)
    {
        var count = note.Milliseconds * sampleRate / 1000;

        for (var i = 0; i < count && cursor < samples.Length; i++, cursor++, phaseIndex++)
        {
            if (note.Frequency <= 0)
            {
                continue;
            }

            // Absolute phase keeps tone continuity between same-pitch notes.
            var phase = phaseIndex * 2.0 * Math.PI * note.Frequency / sampleRate;
            var envelope = Math.Min(1.0, Math.Min(i, count - i) / (double)fade);
            samples[cursor] = (short)(Math.Sin(phase) * amplitude * envelope);
        }
    }

    return samples;
}

// Super Mario Bros. theme (overworld), transcribed from the original
// Console.Beep note sequence. Composition follows the section order of
// that implementation: main lick twice, bridge, then the C-section
// motif with its repeats.
static (int Frequency, int Milliseconds)[] MarioTheme()
{
    (int Frequency, int Milliseconds)[] A1 =
    [
        (659, 125), (659, 125), (0, 125), (659, 125), (0, 167), (523, 125),
        (659, 125), (0, 125), (784, 125),
        (0, 375), (392, 125), (0, 375), (523, 125),
        (0, 250), (392, 125), (0, 250), (330, 125), (0, 250), (440, 125),
        (0, 125), (494, 125), (0, 125), (466, 125), (0, 42), (440, 125),
        (0, 125), (392, 125), (0, 125), (659, 125), (0, 125), (784, 125),
        (0, 125), (880, 125), (0, 125), (698, 125),
        (784, 125), (0, 125), (659, 125), (0, 125), (523, 125), (0, 125),
        (587, 125), (494, 125), (0, 125), (523, 125),
    ];

    (int Frequency, int Milliseconds)[] A2 =
    [
        (0, 250), (392, 125), (0, 250), (330, 125), (0, 250), (440, 125),
        (0, 125), (494, 125), (0, 125), (466, 125), (0, 42), (440, 125),
        (0, 125), (392, 125), (0, 125), (659, 125), (0, 125), (784, 125),
        (0, 125), (880, 125), (0, 125), (698, 125),
        (784, 125), (0, 125), (659, 125), (0, 125), (523, 125), (0, 125),
        (587, 125), (494, 125),
    ];

    (int Frequency, int Milliseconds)[] B =
    [
        (0, 375), (784, 125), (740, 125), (698, 125),
        (0, 42), (622, 125),
        (0, 125), (659, 125),
        (0, 167), (415, 125), (440, 125), (523, 125),
        (0, 125), (440, 125), (523, 125), (587, 125),
        (0, 250), (784, 125), (740, 125), (698, 125),
        (0, 42), (622, 125),
        (0, 125), (659, 125),
        (0, 167), (698, 125), (0, 125), (698, 125), (698, 125), (0, 625),
    ];

    (int Frequency, int Milliseconds)[] M1 =
    [
        (784, 125), (740, 125), (698, 125), (0, 42), (622, 125), (0, 125),
        (659, 125), (0, 167), (415, 125), (440, 125), (523, 125), (0, 125),
        (440, 125), (523, 125), (587, 125), (0, 250),
    ];

    (int Frequency, int Milliseconds)[] M2 =
    [
        (784, 125), (740, 125), (698, 125), (0, 42), (622, 125), (0, 125),
        (659, 125), (0, 167), (698, 125), (0, 125), (698, 125), (698, 125),
        (0, 625),
    ];

    (int Frequency, int Milliseconds)[] M4 =
    [
        (622, 125), (0, 250), (587, 125), (0, 250), (523, 125),
    ];

    return
    [
        .. A1, .. A2, .. B, .. M1, .. M4, (0, 1125),
        .. M1, .. M2, .. M1, .. M4,
    ];
}

// Korobeiniki (Tetris A-theme), transcribed from the original
// Console.Beep note sequence: lead-in, two passes of the main part, two
// passes of the lower part, and the long finale twice.
static (int Frequency, int Milliseconds)[] TetrisTheme()
{
    (int Frequency, int Milliseconds)[] Main =
    [
        (1320, 500), (990, 250), (1056, 250), (1188, 250), (1320, 125),
        (1188, 125), (1056, 250), (990, 250), (880, 500), (880, 250),
        (1056, 250), (1320, 500), (1188, 250), (1056, 250), (990, 750),
        (1056, 250), (1188, 500), (1320, 500), (1056, 500), (880, 500),
        (880, 500),
    ];

    (int Frequency, int Milliseconds)[] Lower =
    [
        (1188, 500), (1408, 250), (1760, 500), (1584, 250), (1408, 250),
        (1320, 750), (1056, 250), (1320, 500), (1188, 250), (1056, 250),
        (990, 500), (990, 250), (1056, 250), (1188, 500), (1320, 500),
        (1056, 500), (880, 500), (880, 500),
    ];

    (int Frequency, int Milliseconds)[] Finale =
    [
        (660, 1000), (528, 1000), (594, 1000), (495, 1000), (528, 1000),
        (440, 1000), (419, 1000), (495, 1000), (660, 1000), (528, 1000),
        (594, 1000), (495, 1000), (528, 500), (660, 500), (880, 1000),
        (838, 2000),
    ];

    const int LeadInFrequency = 658;

    return
    [
        (LeadInFrequency, 125),
        .. Main, (0, 250),
        .. Lower, (0, 500),
        .. Main, (0, 250),
        .. Lower, (0, 500),
        .. Finale, .. Finale,
    ];
}

// Zelda - Song of Storms (the windmill hut loop), transcribed from the
// in-game melody: notes exact, rhythm approximated with the swung
// short-short-long feel, and the loop played twice like the source.
// Press any key to stop early.
static (int Frequency, int Milliseconds)[] StormsTheme()
{
    (int Frequency, int Milliseconds)[] Loop =
    [
        // D4 F4 D5, D4 F4 D5
        (294, 170), (349, 170), (587, 480),
        (294, 170), (349, 170), (587, 480),

        // E5 F5 E5 F5 E5 C5 A4(held)
        (659, 140), (698, 140), (659, 140), (698, 140), (659, 140),
        (523, 140), (440, 700),

        // A4 D4 F4 G4 A4(held)
        (440, 170), (294, 170), (349, 400), (392, 170), (440, 700),

        // A4 D4 F4 E5 D5(held), short breath before the loop repeats
        (440, 170), (294, 170), (349, 400), (659, 170), (587, 700), (0, 250),
    ];

    return [.. Loop, .. Loop];
}
