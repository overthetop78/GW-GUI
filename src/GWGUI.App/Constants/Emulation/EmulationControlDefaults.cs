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
    /// <summary>Nombre d'octets dans un mébioctet pour les présélections de capacité.</summary>
    internal const long BytesPerMebibyte = 1024 * 1024;

    /// <summary>Présélections génériques ; chaque format les filtre selon ses capacités autorisées.</summary>
    internal static IReadOnlyList<long> HardDiskCapacityMebibytes { get; } = Array.AsReadOnly<long>([20, 40, 80, 120, 250, 500, 1024, 2047]);
}
