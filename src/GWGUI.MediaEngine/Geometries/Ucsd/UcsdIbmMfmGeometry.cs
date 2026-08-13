using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction;

namespace GWGUI.MediaEngine.Geometries.Ucsd;

/// <summary>Définit la géométrie logique des images UCSD au format IBM MFM.</summary>
internal static class UcsdIbmMfmGeometry
{
    /// <summary>Taille d'un secteur logique UCSD.</summary>
    public const int BlockSize = 512;

    /// <summary>Nombre de cylindres de l'image 160 Kio.</summary>
    public const int CylinderCount = 40;

    /// <summary>Nombre de faces de l'image.</summary>
    public const int HeadCount = 1;

    /// <summary>Nombre de secteurs logiques par cylindre.</summary>
    public const int LogicalSectorsPerCylinder = 8;

    /// <summary>Géométrie complète utilisée par le Reader et le Writer bruts.</summary>
    public static RegularSectorGeometry SectorGeometry { get; } = new(DiskImageFormatIds.UcsdIbmMfm, BlockSize, CylinderCount, HeadCount, LogicalSectorsPerCylinder, 1);
}
