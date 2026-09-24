
using IMediaSectorImage = global::GWGUI.MediaFileSystems.Interfaces.IMediaSectorImage;

namespace GWGUI.MediaFileSystems.FileSystems.Commodore.Dos;

/// <summary>Accède aux secteurs Commodore DOS après conversion explicite de leurs coordonnées.</summary>
internal static class CommodoreDosSectorReader
{
    /// <summary>Tente de récupérer un secteur complet sans masquer sa coordonnée logique.</summary>
    public static bool TryRead(IMediaSectorImage image, int track, int sector, out IReadOnlyList<byte> data)
        => Read(image, track, sector, out data) == CommodoreDosSectorReadStatus.Success;

    /// <summary>Récupère un secteur en distinguant coordonnée invalide, absence et taille incorrecte.</summary>
    public static CommodoreDosSectorReadStatus Read(IMediaSectorImage image, int track, int sector, out IReadOnlyList<byte> data)
    {
        data = [];
        if (!CommodoreDosGeometry.TryToLogicalBlock(image, track, sector, out var logicalBlock)) return CommodoreDosSectorReadStatus.InvalidCoordinate;
        if (!image.TryGetBlock(logicalBlock, out var block)) return CommodoreDosSectorReadStatus.Missing;
        data = block.Data;
        if (block.Data.Count != CommodoreDosLayout.SectorSize) return CommodoreDosSectorReadStatus.Truncated;
        return CommodoreDosSectorReadStatus.Success;
    }
}
