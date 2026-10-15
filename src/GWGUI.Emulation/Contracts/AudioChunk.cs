namespace GWGUI.Emulation.Contracts;

public sealed record AudioChunk(ReadOnlyMemory<short> InterleavedSamples, int SampleRate, int FrameCount, long Sequence, TimeSpan Timestamp, EmulationAudioChannelLayout ChannelLayout = EmulationAudioChannelLayout.Stereo)
{
    public int ChannelCount => System.Numerics.BitOperations.PopCount((uint)ChannelLayout);
    public bool HasValidLength => SampleRate > 0 && FrameCount >= 0 && ChannelCount is >= 1 and <= 8
        && (ChannelLayout & ~EmulationAudioChannelLayout.Surround71) == 0
        && InterleavedSamples.Length == (long)FrameCount * ChannelCount;
}
