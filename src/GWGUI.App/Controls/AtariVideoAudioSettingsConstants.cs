namespace GWGUI.App.Controls;

internal static class AtariVideoAudioSettingsConstants
{
    internal static readonly IReadOnlySet<string> PalApplicationCultures =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "cs-CZ", "da-DK", "de-DE", "el-GR", "es-ES", "fi-FI", "fr-FR", "hu-HU", "it-IT",
            "nb-NO", "nl-NL", "pl-PL", "pt-PT", "ro-RO", "ru-RU", "sv-SE", "tr-TR", "uk-UA"
        };

    internal const string VideoTabResource = "Emulation.Tab.Video";
    internal const string AudioTabResource = "Emulation.Tab.Audio";
    internal const string VideoStandardResource = "Emulation.Video.Standard";
    internal const string RegionResource = "Emulation.Atari.Video.Region";
    internal const string ResolutionResource = "Emulation.Video.Resolution";
    internal const string AspectRatioResource = "Emulation.Video.AspectRatio";
    internal const string CropResource = "Emulation.Video.Crop";
    internal const string FrameSkipResource = "Emulation.Video.FrameSkip";
    internal const string RenderingResource = "Emulation.Video.Settings.Rendering";
    internal const string AudioEnabledResource = "Emulation.Audio.Enabled";
    internal const string AudioOutputResource = "Emulation.Audio.Output";
    internal const string AudioLatencyResource = "Emulation.Audio.Latency";
    internal const string AudioVolumeResource = "Explorer.Volume";
    internal const string AudioQualityResource = "Emulation.Audio.Quality";
    internal const string DefaultAudioOutputResource = "Emulation.Audio.DefaultOutput";
    internal const string AutomaticResource = "Visual.Automatic";
    internal const string DisabledResource = "Emulation.Value.Disabled";
    internal const string EnabledResource = "Emulation.Value.Enabled";
    internal const string CoreManagedResource = "Emulation.Core.Name";
    internal const string StandardOptionKey = "gwgui_atari_video_standard";
    internal const string RegionOptionKey = AtariHardwareSettingsConstants.RegionOptionKey;
    internal const string ResolutionOptionKey = "gwgui_atari_video_resolution";
    internal const string AspectRatioOptionKey = "gwgui_atari_video_aspect_ratio";
    internal const string CropOptionKey = "gwgui_atari_video_crop";
    internal const string FrameSkipOptionKey = "gwgui_atari_video_frameskip";
    internal const string AudioOutputOptionKey = "gwgui_atari_audio_output";
    internal const string AudioLatencyOptionKey = "gwgui_atari_audio_latency";
    internal const string AudioVolumeOptionKey = "gwgui_atari_audio_volume";
    internal const string AudioQualityOptionKey = "gwgui_atari_audio_quality";
    internal const string FloppySoundOptionKey = "hatari_floppy_sound";
    internal const string FloppySoundVolumeOptionKey = "hatari_floppy_sound_volume";
    internal const string PolarizedFilterOptionKey = "hatari_polarized_filter";
    internal const string AutomaticValue = "auto";
    internal const string NativeValue = "native";
    internal const string PixelAspectValue = "pixel";
    internal const string FourByThreeValue = "4:3";
    internal const string EnabledValue = "enabled";
    internal const string DisabledValue = "disabled";
    internal const string DefaultOutputValue = "default";
    internal const string LowQualityValue = "low";
    internal const string NormalQualityValue = "normal";
    internal const string HighQualityValue = "high";
    internal const string PalValue = "PAL";
    internal const string NtscValue = "NTSC";
    internal const string MonochromeValue = "Monochrome";
    internal const int MinimumFrameSkip = 0;
    internal const int MaximumFrameSkip = 5;
    internal const int FrameSkipStep = 1;
    internal const int MinimumLatencyMilliseconds = 20;
    internal const int MaximumLatencyMilliseconds = 100;
    internal const int LatencyStepMilliseconds = 10;
    internal const int MinimumVolumePercent = 0;
    internal const int MaximumVolumePercent = 100;
    internal const int VolumeStepPercent = 10;
    internal const int InclusiveEndpointCount = 1;
}
