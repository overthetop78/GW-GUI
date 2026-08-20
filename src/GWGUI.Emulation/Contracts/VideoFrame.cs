namespace GWGUI.Emulation;

public sealed record VideoFrame(
    ReadOnlyMemory<byte> Pixels,
    int Width,
    int Height,
    int Pitch,
    EmulationPixelFormat PixelFormat,
    float AspectRatio,
    long Sequence,
    TimeSpan Timestamp);
