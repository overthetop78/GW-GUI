using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Cores;

internal static class AtariAudioFunctions
{
    internal static int MaximumBufferedFrames(int sampleRate) =>
        Math.Max(AtariAudioConstants.MinimumBufferedFrameCount,
            sampleRate / AtariAudioConstants.BufferDurationDivisor);

    internal static short[] SingleFrame(short left, short right) =>
        [left, right];

    internal static short[] CopyBatch(nint data, int frameCount)
    {
        var samples = GC.AllocateUninitializedArray<short>(
            checked(frameCount * AtariAudioConstants.StereoChannelCount));
        Marshal.Copy(data, samples, AtariConstants.FirstBufferIndex, samples.Length);
        return samples;
    }

    internal static AudioChunk RetainNewestFrames(AudioChunk chunk, int maximumFrames)
    {
        if (chunk.FrameCount <= maximumFrames) return chunk;
        var firstSample = checked((chunk.FrameCount - maximumFrames) * AtariAudioConstants.StereoChannelCount);
        return chunk with
        {
            InterleavedStereo = chunk.InterleavedStereo[firstSample..].ToArray(),
            FrameCount = maximumFrames
        };
    }
}
