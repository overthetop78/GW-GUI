using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Geometries.Apple;

/// <summary>Définit la géométrie linéaire Macintosh MFM de 1,44 Mio, distincte des géométries GCR.</summary>
public static class MacintoshMfmGeometry
{
    /// <summary>Taille d'un secteur en octets.</summary>
    public const int SectorSize = 512;
    /// <summary>Nombre de cylindres.</summary>
    public const int CylinderCount = DiskGeometryConstants.EightyTrackCylinderCount;
    /// <summary>Nombre de faces.</summary>
    public const int HeadCount = DiskGeometryConstants.DoubleSidedHeadCount;
    /// <summary>Nombre de secteurs par piste.</summary>
    public const int SectorsPerTrack = 18;
    /// <summary>Nombre total de secteurs.</summary>
    public const int SectorCount = CylinderCount * HeadCount * SectorsPerTrack;
    /// <summary>Capacité totale en octets.</summary>
    public const int Capacity = SectorCount * SectorSize;
}
