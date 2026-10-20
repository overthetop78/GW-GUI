using System.Runtime.InteropServices;
using System.Globalization;

namespace GWGUI.Emulation.Functions;

internal static class EmulationAudioFunctions
{
    internal static string? ResolveRuntimeOutput(IReadOnlyDictionary<string, string> options, string key, string defaultOutput)
    {
        var output = options.GetValueOrDefault(key);
        if (output == defaultOutput) output = null;
        return output;
    }

    internal static int ResolveRuntimeLatency(IReadOnlyDictionary<string, string> options, string key) =>
        options.TryGetValue(key, out var selectedLatency)
            && int.TryParse(selectedLatency, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed : EmulationSettingConstants.DefaultAudioLatencyMilliseconds;

    internal static void WriteVolume(ref IAudioOutput? output, ref float volume, ReadOnlySpan<short> samples)
    {
        if (output is null) return;
        if (volume >= 1f)
        {
            output.Write(samples);
            return;
        }

        var scaled = new short[samples.Length];
        for (var index = 0; index < samples.Length; index++)
            scaled[index] = (short)Math.Clamp(samples[index] * volume, short.MinValue, short.MaxValue);
        output.Write(scaled);
    }

    internal static int MaximumBufferedFrames(int sampleRate) => Math.Max(1, sampleRate / 5);

    internal static AudioChunk RetainNativeNewestFrames(AudioChunk chunk, int maximumFrames, bool multichannel)
    {
        if (chunk.FrameCount <= maximumFrames) return chunk;
        var retainedSamples = chunk.InterleavedSamples.Slice(
            (chunk.FrameCount - maximumFrames) * (multichannel ? chunk.ChannelCount : 2)).ToArray();
        return multichannel
            ? chunk with { InterleavedSamples = retainedSamples, FrameCount = maximumFrames }
            : new AudioChunk(retainedSamples, chunk.SampleRate, maximumFrames, chunk.Sequence, chunk.Timestamp);
    }

    internal static short[] CopyBatch(nint data, int frameCount)
        => CopyBatch(data, GC.AllocateUninitializedArray<short>(checked(frameCount * 2)));

    internal static short[] CopyBatch(nint data, nuint frameCount)
        => CopyBatch(data, frameCount, 2);

    internal static short[] CopyBatch(nint data, nuint frameCount, uint channelCount)
        => CopyBatch(data, new short[checked((int)frameCount * (int)channelCount)]);

    private static short[] CopyBatch(nint data, short[] samples)
    {
        Marshal.Copy(data, samples, 0, samples.Length);
        return samples;
    }

    internal static short[] SingleFrame(short left, short right) => [left, right];

    internal static AudioChunk RetainNewestFrames(AudioChunk chunk, int maximumFrames)
    {
        if (chunk.FrameCount <= maximumFrames) return chunk;
        var firstSample = checked((chunk.FrameCount - maximumFrames) * 2);
        return chunk with
        {
            InterleavedSamples = chunk.InterleavedSamples[firstSample..].ToArray(),
            FrameCount = maximumFrames
        };
    }

    internal static float NormalizeVolume(float volume) =>
        Math.Clamp(volume, 0f, 1f);

    internal static ReadOnlySpan<short> ApplyVolume(ReadOnlySpan<short> samples, float volume, ref short[] buffer)
    {
        if (volume >= 1f) return samples;
        if (buffer.Length < samples.Length) buffer = GC.AllocateUninitializedArray<short>(samples.Length);
        for (var index = 0; index < samples.Length; index++)
            buffer[index] = (short)Math.Clamp((int)MathF.Round(samples[index] * volume),
                short.MinValue, short.MaxValue);
        return buffer.AsSpan(0, samples.Length);
    }
}
