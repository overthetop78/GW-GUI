namespace GWGUI.Emulation.Atari.Constants;

internal static class AtariVideoAudioSettingsConstants
{
    internal const string StandardOption = AtariConfigurationOptionConstants.VideoStandard;
    internal const string ResolutionOption = AtariConfigurationOptionConstants.VideoResolution;
    internal const string AspectRatioOption = "gwgui_atari_video_aspect_ratio";
    internal const string CropOption = AtariMachineOptionConstants.Crop;
    internal const string FrameSkipOption = AtariMachineOptionConstants.Frames;
    internal const string AudioOutputOption = AtariConfigurationOptionConstants.AudioOutput;
    internal const string AudioLatencyOption = AtariConfigurationOptionConstants.AudioLatency;
    internal const string AudioVolumeOption = AtariConfigurationOptionConstants.AudioVolume;
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
