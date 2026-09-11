using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Functions;

/// <summary>Provides phase-independent two-frequency FSK classification and configurable UART frame decoding.</summary>
public static class FskSerialFunctions
{
    public static void AppendPcmCarrier(
        List<short> samples,
        ref short level,
        int sampleRate,
        int baudRate,
        int zeroFrequency,
        int oneFrequency,
        double durationSeconds)
    {
        ArgumentNullException.ThrowIfNull(samples);
        if (!double.IsFinite(durationSeconds) || durationSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));
        var bitCount = checked((int)Math.Round(durationSeconds * baudRate));
        for (var bit = 0; bit < bitCount; bit++)
            AppendPcmBit(samples, ref level, sampleRate, baudRate, zeroFrequency, oneFrequency, true);
    }

    public static void AppendPcmFrame(
        List<short> samples,
        ref short level,
        int sampleRate,
        int baudRate,
        int zeroFrequency,
        int oneFrequency,
        int dataBits,
        int stopBits,
        byte value)
    {
        ArgumentNullException.ThrowIfNull(samples);
        if (dataBits is < 1 or > 8) throw new ArgumentOutOfRangeException(nameof(dataBits));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stopBits);
        AppendPcmBit(samples, ref level, sampleRate, baudRate, zeroFrequency, oneFrequency, false);
        for (var bit = 0; bit < dataBits; bit++)
            AppendPcmBit(samples, ref level, sampleRate, baudRate, zeroFrequency, oneFrequency,
                (value & 1 << bit) != 0);
        for (var stop = 0; stop < stopBits; stop++)
            AppendPcmBit(samples, ref level, sampleRate, baudRate, zeroFrequency, oneFrequency, true);
    }

    public static FskSerialDecodeResult Decode(
        IReadOnlyList<int> samples,
        int sampleRate,
        int baudRate,
        int zeroFrequency,
        int oneFrequency,
        int dataBits,
        int stopBits,
        int minimumSamplesPerBit)
    {
        ArgumentNullException.ThrowIfNull(samples);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(baudRate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(zeroFrequency);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(oneFrequency);
        if (dataBits is < 1 or > 8) throw new ArgumentOutOfRangeException(nameof(dataBits));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stopBits);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimumSamplesPerBit);

        var samplesPerBit = sampleRate / (double)baudRate;
        if (samplesPerBit < minimumSamplesPerBit) return new FskSerialDecodeResult([], 0, 0);
        var best = new FskSerialDecodeResult([], 0, 0);
        for (var phase = 0; phase < Math.Max(1, (int)Math.Round(samplesPerBit)); phase++)
        {
            var bits = new List<bool>();
            for (var start = phase; start + samplesPerBit <= samples.Count; start = (int)Math.Round(start + samplesPerBit))
                bits.Add(ClassifyTone(samples, start, Math.Min(samples.Count, (int)Math.Round(start + samplesPerBit)), sampleRate,
                    zeroFrequency, oneFrequency));
            var candidate = DecodeFrames(bits, dataBits, stopBits);
            if (candidate.ValidFrames > best.ValidFrames
                || candidate.ValidFrames == best.ValidFrames && candidate.Bytes.Count > best.Bytes.Count)
                best = candidate;
        }
        return best;
    }

    private static bool ClassifyTone(
        IReadOnlyList<int> samples,
        int start,
        int end,
        int sampleRate,
        int zeroFrequency,
        int oneFrequency)
    {
        var crossings = 0;
        var previous = samples[start];
        for (var index = start + 1; index < end; index++)
        {
            var current = samples[index];
            if (previous < 0 && current >= 0 || previous >= 0 && current < 0) crossings++;
            previous = current;
        }
        var duration = (end - start) / (double)sampleRate;
        var frequency = duration <= 0 ? 0 : crossings / (2 * duration);
        return Math.Abs(frequency - oneFrequency) < Math.Abs(frequency - zeroFrequency);
    }

    private static void AppendPcmBit(
        List<short> samples,
        ref short level,
        int sampleRate,
        int baudRate,
        int zeroFrequency,
        int oneFrequency,
        bool value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(baudRate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(zeroFrequency);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(oneFrequency);
        var frequency = value ? oneFrequency : zeroFrequency;
        var halfWaveCount = checked((int)Math.Round(frequency * 2d / baudRate));
        var samplesPerHalfWave = Math.Max(1, (int)Math.Round(sampleRate / (frequency * 2d)));
        for (var halfWave = 0; halfWave < halfWaveCount; halfWave++)
        {
            for (var sample = 0; sample < samplesPerHalfWave; sample++) samples.Add(level);
            level = (short)-level;
        }
    }

    public static FskSerialDecodeResult DecodeFrames(IReadOnlyList<bool> bits, int dataBits, int stopBits)
    {
        ArgumentNullException.ThrowIfNull(bits);
        if (dataBits is < 1 or > 8) throw new ArgumentOutOfRangeException(nameof(dataBits));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stopBits);
        var bytes = new List<byte>();
        var tested = 0;
        var valid = 0;
        var frameBits = 1 + dataBits + stopBits;
        for (var index = 0; index + frameBits <= bits.Count;)
        {
            if (bits[index])
            {
                index++;
                continue;
            }
            tested++;
            var validStop = true;
            for (var stop = 0; stop < stopBits; stop++)
                validStop &= bits[index + 1 + dataBits + stop];
            if (!validStop)
            {
                index++;
                continue;
            }
            byte value = 0;
            for (var bit = 0; bit < dataBits; bit++)
                if (bits[index + bit + 1]) value |= (byte)(1 << bit);
            bytes.Add(value);
            valid++;
            index += frameBits;
        }
        return new FskSerialDecodeResult(bytes, valid, tested);
    }
}
