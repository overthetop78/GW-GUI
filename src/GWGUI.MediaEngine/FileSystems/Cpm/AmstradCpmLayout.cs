using GWGUI.MediaEngine.Recognition.Amstrad;

namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Construit les dispositions CP/M propres aux machines Amstrad.</summary>
internal static class AmstradCpmLayout
{
    /// <summary>Premier identifiant de secteur d'un disque CPC système.</summary>
    public const int SystemFirstSectorId = 0xc1;
    /// <summary>Dernier identifiant de secteur d'un disque CPC système.</summary>
    public const int SystemLastSectorId = 0xc9;
    /// <summary>Premier identifiant de secteur d'un disque CPC données.</summary>
    public const int DataFirstSectorId = 0x41;
    /// <summary>Dernier identifiant de secteur d'un disque CPC données.</summary>
    public const int DataLastSectorId = 0x49;
    /// <summary>Nombre d'entrées des répertoires CPC.</summary>
    public const int CpcDirectoryEntries = 64;
    /// <summary>Taille d'allocation CPC.</summary>
    public const int CpcAllocationSize = 1024;
    /// <summary>Nombre de blocs de répertoire CPC.</summary>
    public const int CpcDirectoryBlocks = 2;
    /// <summary>Nombre de pistes réservées d'un disque CPC données.</summary>
    public const int CpcDataReservedTracks = 2;
    /// <summary>Nombre de secteurs par piste CPC.</summary>
    public const int CpcSectorsPerTrack = 9;
    /// <summary>Taille d'un secteur CPC.</summary>
    public const int CpcSectorSize = 512;
    /// <summary>Disposition d'un disque CPC système.</summary>
    public static readonly CpmLayout CpcSystem = new(0, 0, CpcDirectoryEntries, CpcAllocationSize, CpcDirectoryBlocks, false);
    /// <summary>Disposition d'un disque CPC données.</summary>
    public static readonly CpmLayout CpcData = new(CpcDataReservedTracks * CpcSectorsPerTrack * CpcSectorSize, CpcDataReservedTracks * CpcSectorsPerTrack * CpcSectorSize, CpcDirectoryEntries, CpcAllocationSize, CpcDirectoryBlocks, false);

    /// <summary>Construit la disposition PCW depuis sa spécification validée.</summary>
    public static CpmLayout FromPcw(AmstradCpmDiskSpecification specification, int imageLength)
    {
        var origin = checked(specification.ReservedTracks * specification.SectorsPerTrack * specification.SectorSize);
        var allocationCount = Math.Max(0, (imageLength - origin) / specification.AllocationSize);
        return new(origin, origin, specification.DirectoryBlocks * specification.AllocationSize / CpmFormat.DirectoryEntrySize, specification.AllocationSize, specification.DirectoryBlocks, allocationCount > byte.MaxValue);
    }
}
