using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Formats.Floppy.Adf;

/// <summary>Définit la géométrie de l'image ADF Acorn double densité.</summary>
public static class AcornAdfGeometry
{
    /// <summary>Taille d'un bloc Acorn ADF en octets.</summary>
    public const int BlockSize = 1024;
    /// <summary>Nombre de secteurs par piste.</summary>
    public const int SectorsPerTrack = 5;
    /// <summary>Nombre de secteurs par piste des images Archimedes haute densité.</summary>
    public const int HighDensitySectorsPerTrack = 10;
    /// <summary>Nombre d'octets de rembourrage autorisé après la capacité utile.</summary>
    public const int PaddedTrailingByteCount = DataSizeConstants.BytesPerKibibyte;
    /// <summary>Capacité sectorielle utile en octets.</summary>
    public const int Capacity = BlockSize * DiskGeometryConstants.EightyTrackCylinderCount * DiskGeometryConstants.DoubleSidedHeadCount * SectorsPerTrack;
    /// <summary>Taille stockée de la variante rembourrée.</summary>
    public const int PaddedCapacity = Capacity + PaddedTrailingByteCount;
    /// <summary>Capacité sectorielle utile de l'image Archimedes 1600 Kio.</summary>
    public const int HighDensityCapacity = BlockSize * DiskGeometryConstants.EightyTrackCylinderCount * DiskGeometryConstants.DoubleSidedHeadCount * HighDensitySectorsPerTrack;
    /// <summary>Géométrie utile de 800 Kio, hors rembourrage optionnel.</summary>
    public static RegularSectorGeometry Geometry { get; } = new(DiskImageFormatIds.AcornAdfs800, BlockSize, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, SectorsPerTrack);
    /// <summary>Géométrie Archimedes haute densité de 1600 Kio.</summary>
    public static RegularSectorGeometry HighDensityGeometry { get; } = new(DiskImageFormatIds.AcornAdfs1600, BlockSize, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, HighDensitySectorsPerTrack);
}
