using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Geometries.Msx;

/// <summary>Catalogue immuable des quatre géométries de disquettes MSX-DOS.</summary>
public static class MsxDiskGeometryCatalog
{
    /// <summary>Nombre de secteurs par piste des variantes reconnues.</summary>
    public const int SectorsPerTrack = 9;
    /// <summary>Valeur média distinguant le format 1DD à capacité commune avec 2D.</summary>
    public const byte OneDoubleDensityMediaDescriptor = 0xf8;
    /// <summary>Catalogue des quatre géométries.</summary>
    public static IReadOnlyList<MsxDiskGeometry> Supported { get; } = Array.AsReadOnly(new[]
    {
        new MsxDiskGeometry(184_320, DiskImageFormatIds.Msx1D, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount, SectorsPerTrack),
        new MsxDiskGeometry(368_640, DiskImageFormatIds.Msx1Dd, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount, SectorsPerTrack),
        new MsxDiskGeometry(368_640, DiskImageFormatIds.Msx2D, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, SectorsPerTrack),
        new MsxDiskGeometry(737_280, DiskImageFormatIds.Msx2Dd, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, SectorsPerTrack)
    });

    /// <summary>Recherche un profil MSX par son identifiant technique explicite.</summary>
    public static bool TryFromFormatId(string formatId, out MsxDiskGeometry geometry)
    {
        geometry = Supported.SingleOrDefault(candidate => candidate.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase))!;
        return geometry is not null;
    }

    /// <summary>Résout la capacité et le descripteur média en tenant compte de l'ambiguïté des images de 368 640 octets.</summary>
    /// <param name="capacity">Capacité totale de l'image, en octets.</param>
    /// <param name="mediaDescriptor">Descripteur de média lu dans le BPB FAT.</param>
    /// <returns>La géométrie correspondante, ou <see langword="null"/> lorsque la combinaison n'est pas prise en charge.</returns>
    public static MsxDiskGeometry? Find(int capacity, byte mediaDescriptor) => Supported.SingleOrDefault(geometry => geometry.Capacity == capacity && (capacity != 368_640 || (mediaDescriptor == OneDoubleDensityMediaDescriptor) == (geometry.FormatId == DiskImageFormatIds.Msx1Dd)));
}
