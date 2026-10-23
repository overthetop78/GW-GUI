namespace GWGUI.Emulation.Fairchild.Common.Constants;

internal static class AudioConstants
{
    /// <summary>Latence de base du modèle audio local, en millisecondes.</summary>
    internal const int DefaultLatencyMilliseconds = 50;
    /// <summary>Séparation stéréo complète en pourcentage, conservée lorsque ce réglage est applicable au cœur.</summary>
    internal const int FullStereoSeparation = 100;

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
