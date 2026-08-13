using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Geometries.Commodore;

/// <summary>Décrit la géométrie zonée des disquettes Commodore 900.</summary>
public static class Commodore900Geometry
{
    /// <summary>Nombre de cylindres.</summary>
    public const int CylinderCount = DiskGeometryConstants.EightyTrackCylinderCount;
    /// <summary>Nombre de faces.</summary>
    public const int HeadCount = DiskGeometryConstants.DoubleSidedHeadCount;
    /// <summary>Taille d'un secteur en octets.</summary>
    public const int SectorSize = 512;
    /// <summary>Nombre maximal de secteurs par piste.</summary>
    public const int MaximumSectorsPerTrack = 16;
    /// <summary>Nombre minimal de secteurs par piste.</summary>
    public const int MinimumSectorsPerTrack = 13;
    /// <summary>Premier cylindre de la deuxième zone.</summary>
    public const int Zone2StartCylinder = 39;
    /// <summary>Premier cylindre de la troisième zone.</summary>
    public const int Zone3StartCylinder = 53;
    /// <summary>Premier cylindre de la quatrième zone.</summary>
    public const int Zone4StartCylinder = 64;
    /// <summary>Nombre total de blocs physiques.</summary>
    public const int BlockCount = Zone2StartCylinder * HeadCount * 16 + (Zone3StartCylinder - Zone2StartCylinder) * HeadCount * 15 + (Zone4StartCylinder - Zone3StartCylinder) * HeadCount * 14 + (CylinderCount - Zone4StartCylinder) * HeadCount * 13;
    /// <summary>Capacité physique maximale en octets.</summary>
    public const int Capacity = BlockCount * SectorSize;

    /// <summary>Retourne le nombre de secteurs par piste du cylindre indiqué.</summary>
    public static int SectorsPerTrack(int cylinder)
    {
        if (cylinder is < 0 or >= CylinderCount) throw new ArgumentOutOfRangeException(nameof(cylinder), cylinder, $"Le cylindre Commodore 900 doit être compris entre 0 et {CylinderCount - 1}.");
        return cylinder switch { < Zone2StartCylinder => 16, < Zone3StartCylinder => 15, < Zone4StartCylinder => 14, _ => 13 };
    }

    /// <summary>Convertit un bloc logique en adresse physique Commodore 900.</summary>
    public static SectorAddress AddressOf(int logicalBlock)
    {
        if (logicalBlock is < 0 or >= BlockCount) throw new ArgumentOutOfRangeException(nameof(logicalBlock), logicalBlock, $"Le bloc Commodore 900 doit être compris entre 0 et {BlockCount - 1}.");
        var remaining = logicalBlock;
        for (var cylinder = 0; cylinder < CylinderCount; cylinder++)
        {
            var sectors = SectorsPerTrack(cylinder);
            var blocksInCylinder = sectors * HeadCount;
            if (remaining < blocksInCylinder) return new(cylinder, remaining / sectors, remaining % sectors);
            remaining -= blocksInCylinder;
        }
        throw new InvalidOperationException();
    }

    /// <summary>Convertit une adresse physique Commodore 900 en bloc logique.</summary>
    public static int LogicalBlockOf(SectorAddress address)
    {
        if (address.Cylinder is < 0 or >= CylinderCount) throw new ArgumentOutOfRangeException(nameof(address), address, $"Le cylindre Commodore 900 doit être compris entre 0 et {CylinderCount - 1}.");
        if (address.Head is < 0 or >= HeadCount) throw new ArgumentOutOfRangeException(nameof(address), address, $"La face Commodore 900 doit être comprise entre 0 et {HeadCount - 1}.");
        var sectors = SectorsPerTrack(address.Cylinder);
        if (address.Number is < 0 || address.Number >= sectors) throw new ArgumentOutOfRangeException(nameof(address), address, $"Le secteur Commodore 900 doit être compris entre 0 et {sectors - 1} sur ce cylindre.");
        var preceding = 0;
        for (var cylinder = 0; cylinder < address.Cylinder; cylinder++) preceding += SectorsPerTrack(cylinder) * HeadCount;
        return preceding + address.Head * sectors + address.Number;
    }
}
