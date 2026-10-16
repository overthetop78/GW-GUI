using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Philips.Emulators.SameCdi.Constants;

internal static class OptionConstants
{
    internal const string VideoStandard = "same_cdi_video_standard";
    internal const string ReadConfig = "same_cdi_read_config";
    internal const string WriteConfig = "same_cdi_write_config";
    internal const string Saves = "same_cdi_saves";
    internal const string NvramSaves = "same_cdi_nvram_saves";
    internal const string AutoSave = "same_cdi_auto_save";
    internal const string BootFromCli = "same_cdi_boot_from_cli";
    internal const string MamePathsEnable = "same_cdi_mame_paths_enable";
    internal const string AlternateRenderer = "same_cdi_alternate_renderer";
    internal const string Altres = "same_cdi_altres";
    internal const string MouseEnable = "same_cdi_mouse_enable";
    internal const string LightgunMode = "same_cdi_lightgun_mode";
    internal const string ButtonsProfiles = "same_cdi_buttons_profiles";
    internal const string Mame4wayEnable = "same_cdi_mame_4way_enable";
    internal const string Throttle = "same_cdi_throttle";
    internal const string CheatsEnable = "same_cdi_cheats_enable";
    internal const string CpuOverclock = "same_cdi_cpu_overclock";
    internal const string Pal = "pal";
    internal const string Ntsc = "ntsc";
    internal const string Enabled = "enabled";
    internal const string Disabled = "disabled";
    internal const string Game = "game";
    internal const string System = "system";
    internal const string None = "none";
    internal const string Touchscreen = "touchscreen";
    internal const string Lightgun = "lightgun";
    internal const string Value4way = "4way";
    internal const string Strict = "strict";
    internal const string Qbert = "qbert";
    internal const string Default = "default";
    internal const int DefaultResolutionWidth = 640;
    internal const int DefaultResolutionHeight = 480;

    internal static IReadOnlyList<EmulationSettingsNumericRange> CpuPercentRanges { get; } = [new(30, 54, 1, " %"), new(55, 150, 5, " %")];
    internal static IReadOnlyList<(int Width, int Height)> Resolutions { get; } =
    [
        (640, 360), (640, 480), (800, 600), (800, 450), (960, 720), (960, 540),
        (1024, 768), (1024, 576), (1280, 960), (1280, 720), (1600, 1200), (1600, 900),
        (1440, 1080), (1920, 1080), (1920, 1440), (2560, 1440), (2880, 2160), (3840, 2160)
    ];
}
