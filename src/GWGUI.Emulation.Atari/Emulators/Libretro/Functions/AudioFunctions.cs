using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Emulators.Libretro.Functions;

internal static class AudioFunctions
{
    internal static int MaximumBufferedFrames(int sampleRate) =>
        Math.Max(AudioConstants.MinimumBufferedFrameCount,
            sampleRate / AudioConstants.BufferDurationDivisor);

    internal static short[] SingleFrame(short left, short right) =>
        [left, right];

    internal static short[] CopyBatch(nint data, int frameCount)
    {
        var samples = GC.AllocateUninitializedArray<short>(
            checked(frameCount * AudioConstants.StereoChannelCount));
        Marshal.Copy(data, samples, BufferConstants.FirstBufferIndex, samples.Length);
        return samples;
    }

    internal static AudioChunk RetainNewestFrames(AudioChunk chunk, int maximumFrames)
    {
        if (chunk.FrameCount <= maximumFrames) return chunk;
        var firstSample = checked((chunk.FrameCount - maximumFrames) * AudioConstants.StereoChannelCount);
        return chunk with
        {
            InterleavedStereo = chunk.InterleavedStereo[firstSample..].ToArray(),
            FrameCount = maximumFrames
        };
    }
}

internal static class AudioOutputFunctions
{
    internal static float NormalizeVolume(float volume) =>
        Math.Clamp(volume, AudioConstants.MinimumVolume, AudioConstants.MaximumVolume);

    internal static ReadOnlySpan<short> ApplyVolume(ReadOnlySpan<short> samples, float volume, ref short[] buffer)
    {
        if (volume >= AudioConstants.MaximumVolume) return samples;
        if (buffer.Length < samples.Length) buffer = GC.AllocateUninitializedArray<short>(samples.Length);
        for (var index = AudioConstants.FirstSampleIndex; index < samples.Length; index++)
            buffer[index] = (short)Math.Clamp((int)MathF.Round(samples[index] * volume),
                AudioConstants.MinimumSampleValue, AudioConstants.MaximumSampleValue);
        return buffer.AsSpan(AudioConstants.FirstSampleIndex, samples.Length);
    }
}
