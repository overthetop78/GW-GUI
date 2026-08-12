using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Ibm;

namespace GWGUI.MediaEngine.SectorImages.Builders;

/// <summary>Construit une image sectorielle IBM uniforme depuis sa géométrie validée.</summary>
public static class IbmRawSectorImageBuilder
{
    /// <summary>Découpe les octets en secteurs CHS numérotés à partir de un.</summary>
    public static SectorImage Create(ReadOnlyMemory<byte> data, IbmPcGeometry geometry, CancellationToken cancellationToken = default)
    {
        var linear = new LinearSectorImageGeometry(FatBootSectorLayout.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, SectorNumbering.OneBased);
        return LinearSectorImageBuilder.Create(data, geometry.FormatId, linear, cancellationToken);
    }
}
