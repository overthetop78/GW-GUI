namespace GWGUI.Emulation.Atari.Cores;

internal static class AtariAudioConstants
{
    internal const int StereoChannelCount = 2;
    internal const int LeftChannelIndex = 0;
    internal const int RightChannelIndex = 1;
    internal const int SingleFrameCount = 1;
    internal const int BufferDurationDivisor = 5;
    internal const int MinimumBufferedFrameCount = 1;
    internal const int MaximumFramesPerBatch = 64 * 1024;
}
