namespace GWGUI.Emulation.Snk.Emulators.Mame.Constants;

internal static class RuntimeOptionConstants
{
    internal const string ThreadMode = "mame_thread_mode";
    internal const string CheatsEnable = "mame_cheats_enable";
    internal const string Throttle = "mame_throttle";
    internal const string BootToBios = "mame_boot_to_bios";
    internal const string BootToOsd = "mame_boot_to_osd";
    internal const string ReadConfig = "mame_read_config";
    internal const string WriteConfig = "mame_write_config";
    internal const string MamePathsEnable = "mame_mame_paths_enable";
    internal const string Saves = "mame_saves";
    internal const string AutoSave = "mame_auto_save";
    internal const string SoftlistsEnable = "mame_softlists_enable";
    internal const string SoftlistsAutoMedia = "mame_softlists_auto_media";
    internal const string MediaType = "mame_media_type";
    internal const string JoystickDeadzone = "mame_joystick_deadzone";
    internal const string JoystickSaturation = "mame_joystick_saturation";
    internal const string JoystickThreshold = "mame_joystick_threshold";
    internal const string Mame4wayEnable = "mame_mame_4way_enable";
    internal const string ButtonsProfiles = "mame_buttons_profiles";
    internal const string MouseEnable = "mame_mouse_enable";
    internal const string LightgunMode = "mame_lightgun_mode";
    internal const string LightgunOffscreenMode = "mame_lightgun_offscreen_mode";
    internal const string RotationMode = "mame_rotation_mode";
    internal const string AlternateRenderer = "mame_alternate_renderer";
    internal const string Altres = "mame_altres";
    internal const string CpuOverclock = "mame_cpu_overclock";
    internal const string CpuSoundOverclock = "mame_cpu_sound_overclock";
    internal const string Autoloadfastforward = "mame_autoloadfastforward";
    internal const string CoinLimit = "mame_coin_limit";
    internal const string Disabled = "disabled";
    internal const string Enabled = "enabled";
    internal const string RomMedia = "rom";
    internal const string NativeSystemDirectory = "mame";
    internal const string SystemArchive = "neogeo.zip";
    internal const string MvsDriver = "neogeo";
    internal const string CommandExtension = ".cmd";
    internal const bool NeedsFullPath = true;
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".zip", ".7z" });
}
