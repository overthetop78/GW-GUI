namespace GWGUI.Emulation.Atari.Common.Constants;

internal static class AudioConstants
{
    internal const int StereoChannelCount = 2;
    internal const int LeftChannelIndex = 0;
    internal const int RightChannelIndex = 1;
    internal const int SingleFrameCount = 1;
    internal const int BufferDurationDivisor = 5;
    internal const int MinimumBufferedFrameCount = 1;
    internal const int MaximumFramesPerBatch = 64 * 1024;
    internal const float MinimumVolume = 0f;
    internal const float MaximumVolume = 1f;
    internal const float DefaultVolume = MaximumVolume;
    internal const int FirstSampleIndex = 0;
    internal const int MinimumSampleValue = short.MinValue;
    internal const int MaximumSampleValue = short.MaxValue;
}

internal static class VideoAudioSettingsConstants
{
    internal const string StandardOption = ConfigurationOptionConstants.VideoStandard;
    internal const string ResolutionOption = ConfigurationOptionConstants.VideoResolution;
    internal const string AspectRatioOption = "gwgui_atari_video_aspect_ratio";
    internal const string CropOption = MachineOptionConstants.Crop;
    internal const string FrameSkipOption = MachineOptionConstants.Frames;
    internal const string AudioOutputOption = ConfigurationOptionConstants.AudioOutput;
    internal const string AudioLatencyOption = ConfigurationOptionConstants.AudioLatency;
    internal const string AudioVolumeOption = ConfigurationOptionConstants.AudioVolume;
    internal const string AudioQualityOption = "gwgui_atari_audio_quality";
    internal const string FloppySoundOption = "hatari_floppy_sound";
    internal const string FloppySoundVolumeOption = "hatari_floppy_sound_volume";
    internal const string PolarizedFilterOption = "hatari_polarized_filter";

    internal const string Automatic = "auto";
    internal const string Native = "native";
    internal const string PixelAspect = "pixel";
    internal const string FourByThree = "4:3";
    internal const string Enabled = "enabled";
    internal const string Disabled = "disabled";
    internal const string LowQuality = "low";
    internal const string NormalQuality = "normal";
    internal const string HighQuality = "high";

    internal const int MinimumFrameSkip = 0;
    internal const int MaximumFrameSkip = 5;
    internal const int FrameSkipStep = 1;
    internal const int MinimumLatencyMilliseconds = 20;
    internal const int MaximumLatencyMilliseconds = 100;
    internal const int LatencyStepMilliseconds = 10;
    internal const int MinimumVolumePercent = 0;
    internal const int MaximumVolumePercent = 100;
    internal const int VolumeStepPercent = 5;

    internal static readonly IReadOnlyList<int> AudioLatenciesMilliseconds = [20, 35, 50, 75, 100, 150, 250];
    internal static readonly IReadOnlyList<int> FloppySoundVolumesPercent = [25, 50, 75, 100];
}
