namespace GWGUI.Emulation.Sony.Common.Constants;

internal static class AudioConstants
{
    /// <summary>Séparation stéréo complète en pourcentage, conservée lorsque ce réglage est applicable au cœur.</summary>
    internal const int FullStereoSeparation = 100;

    internal const int LeftChannelIndex = 0;
    internal const int RightChannelIndex = 1;
    internal const float MinimumVolume = 0f;
    internal const float MaximumVolume = 1f;
    internal const float DefaultVolume = MaximumVolume;
    internal const int DefaultLatencyMilliseconds = 50;
    internal static IReadOnlyList<int> LatencyChoicesMilliseconds { get; } = [20, 35, 50, 75, 100, 150, 250];
    internal const int FirstSampleIndex = 0;
    internal const int MinimumSampleValue = short.MinValue;
    internal const int MaximumSampleValue = short.MaxValue;
    internal const int DefaultSampleRate = 44100;
}
