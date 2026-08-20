namespace GWGUI.Emulation;

public sealed record AudioChunk(
    ReadOnlyMemory<short> InterleavedStereo,
    int SampleRate,
    int FrameCount,
    long Sequence,
    TimeSpan Timestamp)
{
    public bool HasValidLength => InterleavedStereo.Length == FrameCount * 2;
}
