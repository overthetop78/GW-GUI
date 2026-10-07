namespace GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants;

internal static class StorageConstants
{
    internal const string FloppyLabelPrefix = "FDD";
    internal const string HardDiskLabelPrefix = "HDD";
    internal const int FirstDriveIndex = 0;
    internal const int DriveCount = 2;
    internal static IReadOnlyList<string> FloppyExtensions { get; } = [".d98", ".98d", ".fdi", ".fdd", ".2hd", ".tfd", ".d88", ".88d", ".hdm", ".xdf", ".dup"];
    internal static IReadOnlyList<string> HardDiskExtensions { get; } = [".hdi", ".thd", ".nhd", ".hdd"];
}
