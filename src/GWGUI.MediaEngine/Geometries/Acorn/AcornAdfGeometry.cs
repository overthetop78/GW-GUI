using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Reconstruction;

namespace GWGUI.MediaEngine.Geometries.Acorn;

/// <summary>Définit la géométrie de l'image ADF Acorn double densité.</summary>
public static class AcornAdfGeometry
{
    /// <summary>Taille d'un bloc Acorn ADF en octets.</summary>
    public const int BlockSize = 1024;
    /// <summary>Nombre de secteurs par piste.</summary>
    public const int SectorsPerTrack = 5;
    /// <summary>Nombre d'octets de rembourrage autorisé après la capacité utile.</summary>
    public const int PaddedTrailingByteCount = DataSizeConstants.BytesPerKibibyte;
    /// <summary>Capacité sectorielle utile en octets.</summary>
    public const int Capacity = BlockSize * DiskGeometryConstants.EightyTrackCylinderCount * DiskGeometryConstants.DoubleSidedHeadCount * SectorsPerTrack;
    /// <summary>Taille stockée de la variante rembourrée.</summary>
    public const int PaddedCapacity = Capacity + PaddedTrailingByteCount;
    /// <summary>Géométrie utile de 800 Kio, hors rembourrage optionnel.</summary>
    public static RegularSectorGeometry Geometry { get; } = new(DiskImageFormatIds.AcornAdfs800, BlockSize, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, SectorsPerTrack);
}
