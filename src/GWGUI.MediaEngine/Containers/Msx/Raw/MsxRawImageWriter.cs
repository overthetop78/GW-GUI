using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Msx;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Msx.Raw;

/// <summary>Écrit les images sectorielles MSX DSK 1D, 1DD, 2D et 2DD.</summary>
public sealed class MsxRawImageWriter(LinearSectorImageWriter writer)
{
    /// <summary>Crée un Writer MSX utilisant le Writer linéaire commun.</summary>
    public MsxRawImageWriter() : this(new LinearSectorImageWriter()) { }

    /// <summary>Écrit l'image MSX après validation stricte du profil cible.</summary>
    public Task WriteAsync(SectorImage image, string path, string formatId, CancellationToken cancellationToken = default)
    {
        if (!MsxDiskGeometryCatalog.TryFromFormatId(formatId, out var geometry)) throw MsxRawImageWriterExceptions.UnsupportedFormat(formatId);
        if (image.BlockSize != FatBootSectorLayout.SectorSize || image.Cylinders != geometry.Cylinders || image.Heads != geometry.Heads || image.SectorsPerTrack != geometry.SectorsPerTrack || image.BlockCount * image.BlockSize != geometry.Capacity) throw MsxRawImageWriterExceptions.GeometryMismatch(image.FormatId, formatId);
        var expected = new RegularSectorGeometry(formatId, FatBootSectorLayout.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack);
        return writer.WriteAsync(image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) ? image : image.WithFormatId(formatId), path, expected, cancellationToken);
    }
}
