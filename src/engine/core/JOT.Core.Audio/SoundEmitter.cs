using System.Numerics;
using JOT.Infrastructure.Silk.Audio;

namespace JOT.Core.Audio;

/// <summary>
/// Represents a positional sound in the 3D audio world.
/// </summary>
public sealed class SoundEmitter
{
    private readonly IAudioSource _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundEmitter"/> class.
    /// </summary>
    /// <param name="device">Audio device used for playback.</param>
    /// <param name="samples">
    /// Signed 16-bit PCM samples: mono when <paramref name="channels"/> is
    /// one, interleaved channel pairs when it is two.
    /// </param>
    /// <param name="sampleRate">Sample rate in hertz.</param>
    /// <param name="channels">Channel count: one for mono, two for stereo.</param>
    public SoundEmitter(
        IAudioDevice device,
        short[] samples,
        int sampleRate,
        int channels)
    {
        ArgumentNullException.ThrowIfNull(device);

        _source = device.CreateSource(
            samples,
            sampleRate,
            channels);
    }

    /// <summary>
    /// Gets or sets the emitter position in world space.
    /// </summary>
    public Vector3 Position
    {
        get => _source.Position;
        set => _source.Position = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the emitter repeats
    /// automatically after finishing.
    /// </summary>
    public bool Looping
    {
        get => _source.Looping;
        set => _source.Looping = value;
    }

    /// <summary>
    /// Gets a value indicating whether the emitter is currently playing.
    /// </summary>
    public bool IsPlaying => _source.IsPlaying;

    /// <summary>
    /// Starts playing the emitter sound.
    /// </summary>
    public void Play()
    {
        _source.Play();
    }

    /// <summary>
    /// Stops playing the emitter sound and rewinds it.
    /// </summary>
    public void Stop()
    {
        _source.Stop();
    }
}
