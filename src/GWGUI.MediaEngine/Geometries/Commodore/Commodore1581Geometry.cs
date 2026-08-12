using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Geometries.Commodore;

/// <summary>Décrit les vues logique D81 et physique MFM d'une disquette Commodore 1581.</summary>
public static class Commodore1581Geometry
{
    /// <summary>Nom du format physique utilisé dans les diagnostics de reconstruction.</summary>
    public const string StructureDescriptionName = "Commodore 1581 MFM";
    /// <summary>Nombre de cylindres logiques et physiques.</summary>
    public const int LogicalCylinderCount = DiskGeometryConstants.EightyTrackCylinderCount;
    /// <summary>Nombre de têtes du modèle logique D81.</summary>
    public const int LogicalHeadCount = DiskGeometryConstants.SingleSidedHeadCount;
    /// <summary>Taille d'un bloc logique D81.</summary>
    public const int LogicalBlockSize = 256;
    /// <summary>Nombre de blocs logiques par piste D81.</summary>
    public const int LogicalBlocksPerTrack = 40;
    /// <summary>Nombre de faces physiques MFM.</summary>
    public const int PhysicalHeadCount = DiskGeometryConstants.DoubleSidedHeadCount;
    /// <summary>Taille d'un secteur physique MFM.</summary>
    public const int PhysicalSectorSize = 512;
    /// <summary>Nombre de secteurs physiques par piste et par face.</summary>
    public const int PhysicalSectorsPerTrack = 10;
    /// <summary>Nombre de blocs logiques contenus dans un secteur physique.</summary>
    public const int LogicalBlocksPerPhysicalSector = PhysicalSectorSize / LogicalBlockSize;

    /// <summary>Convertit une piste D81 indexée à un et son secteur logique en bloc logique.</summary>
    public static int ToLogicalBlock(int track, int sector)
    {
        if (track is < 1 or > LogicalCylinderCount) throw CommodoreGeometryExceptions.InvalidTrack(track, 1, LogicalCylinderCount);
        if (sector is < 0 or >= LogicalBlocksPerTrack) throw CommodoreGeometryExceptions.InvalidSector(sector, 0, LogicalBlocksPerTrack - 1);
        return (track - 1) * LogicalBlocksPerTrack + sector;
    }

    /// <summary>Convertit un bloc logique en piste D81 indexée à un et secteur logique indexé à zéro.</summary>
    public static (int Track, int Sector) FromLogicalBlock(int logicalBlock)
    {
        var blockCount = LogicalCylinderCount * LogicalBlocksPerTrack;
        if (logicalBlock is < 0 || logicalBlock >= blockCount) throw CommodoreGeometryExceptions.InvalidLogicalBlock(logicalBlock, blockCount);
        return (logicalBlock / LogicalBlocksPerTrack + 1, logicalBlock % LogicalBlocksPerTrack);
    }

    /// <summary>Calcule le premier bloc logique d'un secteur physique 1581, en conservant l'inversion actuelle des faces SCP.</summary>
    public static int PhysicalSectorToLogicalBlock(int cylinder, int head, int sectorNumber) => cylinder * LogicalBlocksPerTrack + ((head ^ 1) * PhysicalSectorsPerTrack + sectorNumber - 1) * LogicalBlocksPerPhysicalSector;
}
