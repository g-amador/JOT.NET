using JOT.Core.Math.Transformations;
using JOT.Infrastructure.Silk.Audio;

namespace JOT.Core.Audio;

/// <summary>
/// Represents the audio listener at the center of the 3D audio world.
///
/// The listener follows a transform; push its current state into the
/// audio device with <see cref="Synchronize"/>.
/// </summary>
public sealed class Listener
{
    private readonly IAudioDevice _device;

    /// <summary>
    /// Initializes a new instance of the <see cref="Listener"/> class.
    /// </summary>
    /// <param name="device">Audio device used for playback.</param>
    public Listener(IAudioDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        _device = device;
    }

    /// <summary>
    /// Gets the listener transform.
    /// </summary>
    public Transform Transform { get; } = new();

    /// <summary>
    /// Pushes the current transform into the audio device.
    /// </summary>
    public void Synchronize()
    {
        _device.SetListenerPosition(Transform.Position);
        _device.SetListenerOrientation(Transform.Forward, Transform.Up);
    }
}
