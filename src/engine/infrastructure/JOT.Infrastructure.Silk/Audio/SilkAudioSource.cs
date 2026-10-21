using Silk.NET.OpenAL;
using System.Numerics;

namespace JOT.Infrastructure.Silk.Audio;

/// <summary>
/// Silk.NET OpenAL implementation of a 3D sound source.
/// </summary>
internal sealed class SilkAudioSource : IAudioSource
{
    private readonly AL _al;

    private readonly uint _source;

    private Vector3 _position;

    private bool _looping;

    /// <summary>
    /// Initializes a new instance of the <see cref="SilkAudioSource"/> class.
    /// </summary>
    /// <param name="al">OpenAL API instance.</param>
    /// <param name="source">OpenAL source name.</param>
    public SilkAudioSource(
        AL al,
        uint source)
    {
        ArgumentNullException.ThrowIfNull(al);

        _al = al;
        _source = source;
    }

    /// <inheritdoc/>
    public Vector3 Position
    {
        get => _position;
        set
        {
            _al.SetSourceProperty(_source, SourceVector3.Position, value);
            _position = value;
        }
    }

    /// <inheritdoc/>
    public bool Looping
    {
        get => _looping;
        set
        {
            _al.SetSourceProperty(_source, SourceBoolean.Looping, value);
            _looping = value;
        }
    }

    /// <inheritdoc/>
    public bool IsPlaying
    {
        get
        {
            _al.GetSourceProperty(_source, GetSourceInteger.SourceState, out var state);

            return (SourceState)state == SourceState.Playing;
        }
    }

    /// <inheritdoc/>
    public void Play()
    {
        _al.SourcePlay(_source);
    }

    /// <inheritdoc/>
    public void Stop()
    {
        _al.SourceStop(_source);
    }
}
