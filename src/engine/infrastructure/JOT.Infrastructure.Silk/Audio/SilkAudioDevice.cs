using System.Numerics;
using Silk.NET.OpenAL;

namespace JOT.Infrastructure.Silk.Audio;

/// <summary>
/// Silk.NET OpenAL implementation of a 3D audio device.
/// </summary>
public sealed class SilkAudioDevice : IAudioDevice
{
    private readonly ALContext _alc;

    private readonly AL _al;

    private readonly unsafe Device* _device;

    private readonly unsafe Context* _context;

    private readonly List<uint> _buffers = [];

    private readonly List<uint> _sources = [];

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SilkAudioDevice"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the native audio device or context cannot be created.
    /// </exception>
    public unsafe SilkAudioDevice()
    {
        _alc = ALContext.GetApi();

        var device = _alc.OpenDevice(null);

        if (device == null)
        {
            _alc.Dispose();
            throw new InvalidOperationException(
                "Failed to open an OpenAL output device.");
        }

        _device = device;

        var context = _alc.CreateContext(_device, null);

        if (context == null)
        {
            _alc.Dispose();
            throw new InvalidOperationException(
                "Failed to create an OpenAL context.");
        }

        _context = context;

        _alc.MakeContextCurrent(_context);

        _al = AL.GetApi();
    }

    /// <inheritdoc/>
    public unsafe IAudioSource CreateSource(
        short[] samples,
        int sampleRate,
        int channels)
    {
        ArgumentNullException.ThrowIfNull(samples);

        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleRate),
                "Sample rate must be positive.");
        }

        if (samples.Length == 0)
        {
            throw new ArgumentException(
                "Samples cannot be empty.",
                nameof(samples));
        }

        var format = channels switch
        {
            1 => BufferFormat.Mono16,
            2 => BufferFormat.Stereo16,
            _ => throw new ArgumentException(
                "Channels must be one (mono) or two (stereo).",
                nameof(channels)),
        };

        var buffer = _al.GenBuffer();

        _al.BufferData(buffer, format, samples, sampleRate);

        var source = _al.GenSource();

        _al.SetSourceProperty(source, SourceInteger.Buffer, (int)buffer);

        _buffers.Add(buffer);
        _sources.Add(source);

        return new SilkAudioSource(_al, source);
    }

    /// <inheritdoc/>
    public void SetListenerPosition(Vector3 position)
    {
        ThrowIfDisposed();

        _al.SetListenerProperty(ListenerVector3.Position, position);
    }

    /// <inheritdoc/>
    public unsafe void SetListenerOrientation(Vector3 forward, Vector3 up)
    {
        ThrowIfDisposed();

        Span<float> orientation =
        [
            forward.X, forward.Y, forward.Z,
            up.X, up.Y, up.Z,
        ];

        fixed (float* values = orientation)
        {
            _al.SetListenerProperty(ListenerFloatArray.Orientation, values);
        }
    }

    /// <inheritdoc/>
    public unsafe void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var source in _sources)
        {
            _al.DeleteSource(source);
        }

        foreach (var buffer in _buffers)
        {
            _al.DeleteBuffer(buffer);
        }

        if (_context != null)
        {
            _alc.MakeContextCurrent(null);
            _alc.DestroyContext(_context);
        }

        if (_device != null)
        {
            _alc.CloseDevice(_device);
        }

        _al.Dispose();
        _alc.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SilkAudioDevice));
        }
    }
}
