namespace GWGUI.Emulation.Sharp.Emulators.Px68k.Constants;

internal static class MediaRuntimeConstants
{
    internal static readonly IReadOnlyList<string> FloppyExtensions = Array.AsReadOnly(new[] { ".dim", ".img", ".d88", ".88d", ".hdm", ".dup", ".2hd", ".xdf" });
    internal static readonly IReadOnlyList<string> HardDiskExtensions = Array.AsReadOnly(new[] { ".hdf" });
    internal const string ConfigurationFileName = "config";
    internal const string ConfigurationSection = "[WinX68k]";
    internal const string FloppyKeyPrefix = "FDD";
    internal const string HardDiskKeyPrefix = "HDD";
}
