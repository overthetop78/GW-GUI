using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Functions;

internal static class EmulationAudioFunctions
{
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
    {
        var samples = GC.AllocateUninitializedArray<short>(checked(frameCount * 2));
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
