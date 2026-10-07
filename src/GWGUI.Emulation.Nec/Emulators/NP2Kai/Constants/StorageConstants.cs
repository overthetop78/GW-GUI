namespace GWGUI.Emulation.Nec.Emulators.NP2Kai.Constants;

internal static class StorageConstants
{
    internal const string FloppyLabelPrefix = "FDD";
    internal const string HardDiskLabelPrefix = "HDD";
    internal const int FirstDriveIndex = 0;
    internal const int DriveCount = 2;
    internal static IReadOnlyList<string> FloppyExtensions { get; } = [".d88", ".88d", ".d98", ".98d", ".fdi", ".xdf", ".hdm", ".dup", ".2hd", ".tfd", ".nfd", ".hd4", ".hd5", ".hd9", ".fdd", ".h01", ".hdb", ".ddb", ".dd6", ".dcp", ".dcu", ".flp", ".img", ".ima", ".bin", ".fim"];
    internal static IReadOnlyList<string> HardDiskExtensions { get; } = [".thd", ".nhd", ".hdi", ".vhd", ".slh", ".hdn"];
}
