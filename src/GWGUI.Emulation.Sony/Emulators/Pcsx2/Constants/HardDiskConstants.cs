namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;

internal static class HardDiskConstants
{
    internal const string NativeFile = "pcsx2/DEV9hdd.raw";
    internal const string Extension = ".raw";
    internal const string ImageExtension = ".img";
    internal const string InterfaceName = "DEV9 (ATA)";
    internal const string FormatId = "pcsx2-dev9";
    internal const int SectorBytes = 512;
    internal const long Gibibyte = 1024L * 1024 * 1024;
    internal const int DefaultCapacityGiB = 40;
    internal static IReadOnlyList<int> CapacitiesGiB { get; } = [8, 16, 20, 40, 80, 120];
}
