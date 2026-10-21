using System.Numerics;

namespace JOT.Infrastructure.Silk.Audio;

/// <summary>
/// Represents a 3D audio output device.
///
/// One device owns a native audio context; dispose it to release the
/// hardware resources.
/// </summary>
public interface IAudioDevice : IDisposable
{
    /// <summary>
    /// Uploads raw audio samples for playback and returns a sound source.
    /// </summary>
    /// <param name="samples">
    /// Signed 16-bit PCM samples: mono when <paramref name="channels"/> is
    /// one, interleaved channel pairs when it is two.
    /// </param>
    /// <param name="sampleRate">
    /// Sample rate in hertz.
    /// </param>
    /// <param name="channels">
    /// Channel count: one for mono, two for stereo.
    /// </param>
    /// <returns>
    /// A source positioned for playback of the uploaded samples.
    /// </returns>
    IAudioSource CreateSource(
        short[] samples,
        int sampleRate,
        int channels);

    /// <summary>
    /// Sets the listener position in world space.
    /// </summary>
    /// <param name="position">
    /// Listener position.
    /// </param>
    void SetListenerPosition(Vector3 position);

    /// <summary>
    /// Sets the listener orientation.
    /// </summary>
    /// <param name="forward">
    /// Direction the listener faces.
    /// </param>
    /// <param name="up">
    /// Direction above the listener.
    /// </param>
    void SetListenerOrientation(Vector3 forward, Vector3 up);
}
