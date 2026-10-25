namespace GWGUI.Emulation.Snk.Emulators.Mame2016.Constants;

internal static class RuntimeOptionConstants
{
    internal const string ReadConfig = "mame2016_read_config";
    internal const string WriteConfig = "mame2016_write_config";
    internal const string Saves = "mame2016_saves";
    internal const string AutoSave = "mame2016_auto_save";
    internal const string MouseEnable = "mame2016_mouse_enable";
    internal const string Throttle = "mame2016_throttle";
    internal const string CheatsEnable = "mame2016_cheats_enable";
    internal const string CpuOverclock = "mame2016_cpu_overclock";
    internal const string AlternateRenderer = "mame2016_alternate_renderer";
    internal const string SoftlistsEnable = "mame2016_softlists_enable";
    internal const string SoftlistsAutoMedia = "mame2016_softlists_auto_media";
    internal const string MediaType = "mame2016_media_type";
    internal const string BootToBios = "mame2016_boot_to_bios";
    internal const string BootToOsd = "mame2016_boot_to_osd";
    internal const string BootFromCli = "mame2016_boot_from_cli";
    internal const string Disabled = "disabled";
    internal const string Enabled = "enabled";
    internal const string RomMedia = "rom";
    internal const string NativeSystemDirectory = "mame2016";
    internal const string SystemArchive = "neogeo.zip";
    internal const string MvsDriver = "neogeo";
    internal const string CommandExtension = ".cmd";
    internal const bool NeedsFullPath = true;
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".zip", ".7z" });
}
