namespace GWGUI.Emulation.Apple.Common.Constants;

internal static class AudioConstants
{
    internal const int LeftChannelIndex = 0;
    internal const int RightChannelIndex = 1;
    internal const int SingleFrameCount = 1;
    internal const int MaximumFramesPerBatch = 64 * 1024;
    internal const float MinimumVolume = 0f;
    internal const float MaximumVolume = 1f;
    internal const float DefaultVolume = MaximumVolume;
    internal const int FirstSampleIndex = 0;
    internal const int MinimumSampleValue = short.MinValue;
    internal const int MaximumSampleValue = short.MaxValue;
}
