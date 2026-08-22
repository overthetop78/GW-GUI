namespace GWGUI.App.Constants.Emulation;

internal static class EmulationControlDefaults
{
    internal const string HardDiskFileName = "Workbench.hdf";
    internal const int HardDiskSizeMiB = 2048;
    internal const int HardDiskHeads = 16;
    internal const int HardDiskSectorsPerTrack = 63;
    internal const int HardDiskBytesPerSector = 512;

    internal const string EraseTrackRange = "c=0-79:h=0-1";
    internal const int EraseRevolutions = 3;
    internal const int CleanCylinders = 80;
    internal const int CleanPasses = 3;
    internal const int CleanLingerMilliseconds = 100;
}
