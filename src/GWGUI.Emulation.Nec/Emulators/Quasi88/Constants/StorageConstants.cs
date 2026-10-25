namespace GWGUI.Emulation.Nec.Emulators.Quasi88.Constants;

internal static class StorageConstants
{
    internal static IReadOnlyDictionary<int, uint> Subsystems { get; } =
        new Dictionary<int, uint> { [2] = 0x0101 };
    internal const string FloppyLabelPrefix = "FDD";
    internal const string HardDiskLabelPrefix = "HDD";
    internal const int FirstDriveIndex = 0;
    internal const int DriveCount = 2;
    internal static IReadOnlyList<string> FloppyExtensions { get; } = [".d88"];
    internal static IReadOnlyList<string> HardDiskExtensions { get; } = [];
}
