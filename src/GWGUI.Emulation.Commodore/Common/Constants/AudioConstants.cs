namespace GWGUI.Emulation.Commodore.Common.Constants;

internal static class AudioConstants
{
    internal const int StereoChannelCount = 2;
    internal const int SingleFrameCount = 1;
    internal const int BufferDurationDivisor = 5;
    internal const int MinimumBufferedFrameCount = 1;
    internal const float MinimumVolume = 0f;
    internal const float MaximumVolume = 1f;
    internal const float DefaultVolume = MaximumVolume;
    internal const int FirstSampleIndex = 0;
}
