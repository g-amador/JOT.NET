using System.Text;

namespace JOT.Core.Audio;

/// <summary>
/// Represents a loaded WAV file.
///
/// Parses uncompressed PCM WAV data (RIFF container) and exposes the
/// samples in signed 16-bit form, ready for positional playback.
/// Compressed formats are intentionally rejected: the audio glue accepts
/// 16-bit PCM only.
/// </summary>
public sealed class WaveFile
{
    private const short PcmFormat = 1;

    private const short SixteenBits = 16;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaveFile"/> class.
    /// </summary>
    /// <param name="samples">Signed 16-bit interleaved PCM samples.</param>
    /// <param name="sampleRate">Sample rate in hertz.</param>
    /// <param name="channels">Channel count: one mono, two stereo.</param>
    public WaveFile(
        short[] samples,
        int sampleRate,
        int channels)
    {
        ArgumentNullException.ThrowIfNull(samples);

        Samples = samples;
        SampleRate = sampleRate;
        Channels = channels;
    }

    /// <summary>
    /// Gets the signed 16-bit interleaved PCM samples.
    /// </summary>
    public short[] Samples { get; }

    /// <summary>
    /// Gets the sample rate in hertz.
    /// </summary>
    public int SampleRate { get; }

    /// <summary>
    /// Gets the channel count: one mono, two stereo.
    /// </summary>
    public int Channels { get; }

    /// <summary>
    /// Gets the playback length in seconds.
    /// </summary>
    public double Seconds =>
        SampleRate > 0
            ? Samples.Length / Channels / (double)SampleRate
            : 0;

    /// <summary>
    /// Loads a WAV file from disk.
    /// </summary>
    /// <param name="path">WAV file path.</param>
    /// <returns>The loaded wave.</returns>
    /// <exception cref="InvalidDataException">
    /// Thrown when the file is not an uncompressed 16-bit PCM WAV file.
    /// </exception>
    public static WaveFile Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return Parse(File.ReadAllBytes(path));
    }

    private static WaveFile Parse(byte[] bytes)
    {
        if (bytes.Length < 12
            || Encoding.ASCII.GetString(bytes, 0, 4) != "RIFF"
            || Encoding.ASCII.GetString(bytes, 8, 4) != "WAVE")
        {
            throw new InvalidDataException("Not a RIFF/WAVE file.");
        }

        short channels = 0;
        var sampleRate = 0;

        var formatSeen = false;
        var dataOffset = -1;
        var dataLength = 0;

        var offset = 12;

        // Chunks appear in any order and may include unknown sections
        // (LIST, fact, cue labels); stride over them, word aligned.
        while (offset + 8 <= bytes.Length)
        {
            var id = Encoding.ASCII.GetString(bytes, offset, 4);
            var size = (int)BitConverter.ToUInt32(bytes, offset + 4);
            var payload = offset + 8;

            switch (id)
            {
                case "fmt " when size >= 16 && !formatSeen:
                    formatSeen = true;
                    channels = BitConverter.ToInt16(bytes, payload + 2);
                    sampleRate = BitConverter.ToInt32(bytes, payload + 4);

                    var format = BitConverter.ToInt16(bytes, payload);
                    if (format != PcmFormat)
                    {
                        throw new InvalidDataException(
                            $"WAV compression format {format} is not supported;"
                            + " only uncompressed 16-bit PCM files are accepted.");
                    }

                    var bits = BitConverter.ToInt16(bytes, payload + 14);
                    if (bits != SixteenBits)
                    {
                        throw new InvalidDataException(
                            $"WAV bit depth {bits} is not supported;"
                            + " only 16-bit PCM files are accepted.");
                    }

                    break;

                case "data" when dataOffset < 0:
                    dataOffset = payload;
                    dataLength = System.Math.Min(size, bytes.Length - payload) & ~1;
                    break;
            }

            offset = payload + size + (size & 1);
        }

        if (!formatSeen)
        {
            throw new InvalidDataException("WAV file has no format chunk.");
        }

        if (dataOffset < 0)
        {
            throw new InvalidDataException("WAV file has no data chunk.");
        }

        var samples = new short[dataLength / 2];

        Buffer.BlockCopy(bytes, dataOffset, samples, 0, dataLength);

        return new WaveFile(samples, sampleRate, channels);
    }
}
