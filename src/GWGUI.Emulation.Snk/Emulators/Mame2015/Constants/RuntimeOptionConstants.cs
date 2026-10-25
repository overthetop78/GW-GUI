namespace GWGUI.Emulation.Snk.Emulators.Mame2015.Constants;

internal static class RuntimeOptionConstants
{
    internal const string ReadConfig = "mame2015_read_config";
    internal const string AutoSave = "mame2015_auto_save";
    internal const string MouseMode = "mame2015_mouse_mode";
    internal const string Throttle = "mame2015_throttle";
    internal const string CheatsEnable = "mame2015_cheats_enable";
    internal const string HideNagscreen = "mame2015_hide_nagscreen";
    internal const string HideInfoscreen = "mame2015_hide_infoscreen";
    internal const string HideWarnings = "mame2015_hide_warnings";
    internal const string AlternateRenderer = "mame2015_alternate_renderer";
    internal const string BootToOsd = "mame2015_boot_to_osd";
    internal const string BootFromCli = "mame2015_boot_from_cli";
    internal const string Disabled = "disabled";
    internal const string NoPointer = "none";
    internal const string SystemArchive = "neogeo.zip";
    internal const string NativeSystemDirectory = "mame2015";
    internal const bool NeedsFullPath = true;
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".zip", ".7z" });
}
