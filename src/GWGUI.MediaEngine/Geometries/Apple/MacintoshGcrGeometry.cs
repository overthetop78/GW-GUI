using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Geometries.Apple;

/// <summary>Définit les géométries Macintosh GCR simple et double face.</summary>
public static class MacintoshGcrGeometry
{
    /// <summary>Taille d'un bloc en octets.</summary>
    public const int BlockSize = 512;
    /// <summary>Nombre de blocs d'une image simple face.</summary>
    public const int SingleSidedBlockCount = 800;
    /// <summary>Nombre de cylindres.</summary>
    public const int CylinderCount = DiskGeometryConstants.EightyTrackCylinderCount;
    /// <summary>Nombre de faces d'une image 400 Kio.</summary>
    public const int SingleSidedHeadCount = DiskGeometryConstants.SingleSidedHeadCount;
    /// <summary>Nombre de faces d'une image 800 Kio.</summary>
    public const int DoubleSidedHeadCount = DiskGeometryConstants.DoubleSidedHeadCount;
    /// <summary>Nombre maximal de secteurs par piste.</summary>
    public const int MaximumSectorsPerTrack = 12;
    /// <summary>Nombre de cylindres par zone de vitesse.</summary>
    public const int ZoneCylinderCount = 16;
    /// <summary>Borne exclusive de la première zone.</summary>
    public const int Zone1End = ZoneCylinderCount;
    /// <summary>Borne exclusive de la deuxième zone.</summary>
    public const int Zone2End = ZoneCylinderCount * 2;
    /// <summary>Borne exclusive de la troisième zone.</summary>
    public const int Zone3End = ZoneCylinderCount * 3;
    /// <summary>Borne exclusive de la quatrième zone.</summary>
    public const int Zone4End = ZoneCylinderCount * 4;
    /// <summary>Borne exclusive de la cinquième zone.</summary>
    public const int Zone5End = CylinderCount;
    /// <summary>Nombre de secteurs de la deuxième zone.</summary>
    public const int Zone2SectorCount = 11;
    /// <summary>Nombre de secteurs de la troisième zone.</summary>
    public const int Zone3SectorCount = 10;
    /// <summary>Nombre de secteurs de la quatrième zone.</summary>
    public const int Zone4SectorCount = 9;
    /// <summary>Nombre de secteurs de la cinquième zone.</summary>
    public const int Zone5SectorCount = 8;
    /// <summary>Capacité simple face en octets.</summary>
    public const int Capacity400K = SingleSidedBlockCount * BlockSize;
    /// <summary>Capacité double face en octets.</summary>
    public const int Capacity800K = Capacity400K * DoubleSidedHeadCount;
    /// <summary>Capacité d'une image Macintosh MFM 1,44 Mio en octets.</summary>
    public const int Capacity1440K = 1_474_560;

    /// <summary>Retourne le nombre de secteurs de la zone contenant le cylindre.</summary>
    public static int Sectors(int cylinder)
    {
        if (cylinder is < 0 or >= CylinderCount) throw AppleGeometryExceptions.InvalidCylinder(cylinder, CylinderCount);
        return cylinder switch { < Zone1End => MaximumSectorsPerTrack, < Zone2End => Zone2SectorCount, < Zone3End => Zone3SectorCount, < Zone4End => Zone4SectorCount, _ => Zone5SectorCount };
    }

    /// <summary>Convertit un bloc logique en cylindre, face et secteur à base zéro pour une ou deux faces.</summary>
    public static SectorAddress Address(int logicalBlock, int heads)
    {
        if (heads is not (SingleSidedHeadCount or DoubleSidedHeadCount)) throw AppleGeometryExceptions.InvalidHeadCount(heads);
        var blockCount = SingleSidedBlockCount * heads;
        if (logicalBlock is < 0 || logicalBlock >= blockCount) throw AppleGeometryExceptions.InvalidMacintoshLogicalBlock(logicalBlock, heads, blockCount);
        var remaining = logicalBlock;
        for (var cylinder = 0; cylinder < CylinderCount; cylinder++)
        {
            var sectors = Sectors(cylinder);
            var perCylinder = sectors * heads;
            if (remaining < perCylinder) return new(cylinder, remaining / sectors, remaining % sectors);
            remaining -= perCylinder;
        }
        throw AppleGeometryExceptions.InvalidMacintoshLogicalBlock(logicalBlock, heads, blockCount);
    }

    /// <summary>Indique si la capacité correspond à une image Macintosh cataloguée.</summary>
    public static bool IsSupportedCapacity(int capacity) => capacity is Capacity400K or Capacity800K or Capacity1440K;
}
