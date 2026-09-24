using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Functions;

/// <summary>Shares integer PCM profile validation and channel extraction between sequential media decoders.</summary>
internal static class PcmSampleFunctions
{
    public static void AppendSilence(List<short> samples, int sampleRate, double durationSeconds)
    {
        ArgumentNullException.ThrowIfNull(samples);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        if (!double.IsFinite(durationSeconds) || durationSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));
        var count = checked((int)Math.Round(sampleRate * durationSeconds));
        for (var index = 0; index < count; index++) samples.Add(0);
    }

    public static bool TryGetProfile(
        IReadOnlyDictionary<string, string> metadata,
        out int channels,
        out int sampleRate,
        out int bitsPerSample,
        out int blockAlign)
    {
        channels = sampleRate = bitsPerSample = blockAlign = 0;
        return TryReadInt(metadata, "channels", out channels)
            && TryReadInt(metadata, "sampleRate", out sampleRate)
            && TryReadInt(metadata, "bitsPerSample", out bitsPerSample)
            && TryReadInt(metadata, "blockAlign", out blockAlign)
            && channels > 0
            && sampleRate > 0
            && bitsPerSample is 8 or 16 or 24 or 32
            && blockAlign == channels * (bitsPerSample / 8);
    }

    public static IReadOnlyList<int> ExtractChannel(
        ReadOnlySpan<byte> pcm,
        int channels,
        int bitsPerSample,
        int blockAlign,
        int channel)
    {
        if (channel < 0 || channel >= channels) throw new ArgumentOutOfRangeException(nameof(channel));
        if (blockAlign <= 0 || pcm.Length % blockAlign != 0)
            throw new InvalidDataException("The PCM byte sequence does not contain complete sample frames.");
        var bytesPerSample = bitsPerSample / 8;
        var samples = new int[pcm.Length / blockAlign];
        for (var frame = 0; frame < samples.Length; frame++)
        {
            var offset = frame * blockAlign + channel * bytesPerSample;
            samples[frame] = bitsPerSample switch
            {
                8 => pcm[offset] - 128,
                16 => BinaryPrimitives.ReadInt16LittleEndian(pcm.Slice(offset, 2)),
                24 => ReadInt24LittleEndian(pcm.Slice(offset, 3)),
                32 => BinaryPrimitives.ReadInt32LittleEndian(pcm.Slice(offset, 4)),
                _ => throw new NotSupportedException($"Integer PCM with {bitsPerSample} bits per sample is unsupported.")
            };
        }
        return samples;
    }

    private static int ReadInt24LittleEndian(ReadOnlySpan<byte> value)
    {
        var result = value[0] | value[1] << 8 | value[2] << 16;
        return (result & 0x00800000) == 0 ? result : result | unchecked((int)0xff000000);
    }

    private static bool TryReadInt(IReadOnlyDictionary<string, string> metadata, string key, out int value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && int.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out value);
    }
}
