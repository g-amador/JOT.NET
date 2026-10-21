using System.Numerics;

namespace JOT.Infrastructure.Silk.Audio;

/// <summary>
/// Represents a playable 3D sound source attached to an audio device.
/// </summary>
public interface IAudioSource
{
    /// <summary>
    /// Gets or sets the source position in world space.
    /// </summary>
    Vector3 Position { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the source repeats
    /// automatically after finishing.
    /// </summary>
    bool Looping { get; set; }

    /// <summary>
    /// Gets a value indicating whether the source is currently playing.
    /// </summary>
    bool IsPlaying { get; }

    /// <summary>
    /// Starts playing the source.
    /// </summary>
    void Play();

    /// <summary>
    /// Stops playing the source and rewinds it.
    /// </summary>
    void Stop();
}
