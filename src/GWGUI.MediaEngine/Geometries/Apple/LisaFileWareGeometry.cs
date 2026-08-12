using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Geometries.Apple;

/// <summary>Définit la géométrie zonée Lisa FileWare.</summary>
public static class LisaFileWareGeometry
{
    /// <summary>Nombre total de blocs logiques.</summary>
    public const int BlockCount = 1702;
    /// <summary>Nombre de cylindres.</summary>
    public const int CylinderCount = 46;
    /// <summary>Nombre de faces.</summary>
    public const int HeadCount = DiskGeometryConstants.DoubleSidedHeadCount;
    /// <summary>Dernier nombre de pistes pouvant encore représenter une image Lisa déjà structurée.</summary>
    public const int LinearTrackThreshold = 84;
    /// <summary>Nombre maximal de secteurs par piste.</summary>
    public const int MaximumSectorsPerTrack = 22;
    /// <summary>Borne exclusive de la première zone.</summary>
    public const int Zone1End = 4;
    /// <summary>Borne exclusive de la deuxième zone.</summary>
    public const int Zone2End = 11;
    /// <summary>Borne exclusive de la troisième zone.</summary>
    public const int Zone3End = 17;
    /// <summary>Borne exclusive de la quatrième zone.</summary>
    public const int Zone4End = 23;
    /// <summary>Borne exclusive de la cinquième zone.</summary>
    public const int Zone5End = 29;
    /// <summary>Borne exclusive de la sixième zone.</summary>
    public const int Zone6End = 35;
    /// <summary>Borne exclusive de la septième zone.</summary>
    public const int Zone7End = 42;
    /// <summary>Borne exclusive de la huitième zone.</summary>
    public const int Zone8End = CylinderCount;
    /// <summary>Nombre de secteurs par piste de la deuxième zone.</summary>
    public const int Zone2SectorCount = 21;
    /// <summary>Nombre de secteurs par piste de la troisième zone.</summary>
    public const int Zone3SectorCount = 20;
    /// <summary>Nombre de secteurs par piste de la quatrième zone.</summary>
    public const int Zone4SectorCount = 19;
    /// <summary>Nombre de secteurs par piste de la cinquième zone.</summary>
    public const int Zone5SectorCount = 18;
    /// <summary>Nombre de secteurs par piste de la sixième zone.</summary>
    public const int Zone6SectorCount = 17;
    /// <summary>Nombre de secteurs par piste de la septième zone.</summary>
    public const int Zone7SectorCount = 16;
    /// <summary>Nombre de secteurs par piste de la huitième zone.</summary>
    public const int Zone8SectorCount = 15;

    /// <summary>Retourne le nombre de secteurs de la zone contenant le cylindre.</summary>
    public static int Sectors(int cylinder)
    {
        if (cylinder is < 0 or >= CylinderCount) throw AppleGeometryExceptions.InvalidCylinder(cylinder, CylinderCount);
        return cylinder switch { < Zone1End => MaximumSectorsPerTrack, < Zone2End => Zone2SectorCount, < Zone3End => Zone3SectorCount, < Zone4End => Zone4SectorCount, < Zone5End => Zone5SectorCount, < Zone6End => Zone6SectorCount, < Zone7End => Zone7SectorCount, _ => Zone8SectorCount };
    }

    /// <summary>Convertit un bloc logique en cylindre, face et secteur tous numérotés à partir de zéro.</summary>
    public static SectorAddress Address(int logicalBlock)
    {
        if (logicalBlock is < 0 or >= BlockCount) throw AppleGeometryExceptions.InvalidLogicalBlock(logicalBlock, BlockCount);
        var sectorsPerSide = BlockCount / HeadCount;
        var head = logicalBlock / sectorsPerSide;
        var remaining = logicalBlock % sectorsPerSide;
        for (var cylinder = 0; cylinder < CylinderCount; cylinder++)
        {
            var count = Sectors(cylinder);
            if (remaining < count) return new(cylinder, head, remaining);
            remaining -= count;
        }
        throw AppleGeometryExceptions.InvalidLogicalBlock(logicalBlock, BlockCount);
    }
}
