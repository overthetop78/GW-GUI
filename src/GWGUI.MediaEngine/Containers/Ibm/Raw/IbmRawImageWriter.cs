using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Ibm.Raw;

/// <summary>Écrit les conteneurs sectoriels IBM IMA et IMG dans l'ordre logique CHS.</summary>
public sealed class IbmRawImageWriter(LinearSectorImageWriter writer)
{
    /// <summary>Crée un Writer IBM utilisant le Writer linéaire commun.</summary>
    public IbmRawImageWriter() : this(new LinearSectorImageWriter()) { }

    /// <summary>Écrit le profil IBM explicitement demandé après validation de tous les blocs.</summary>
    public Task WriteAsync(SectorImage image, string path, string formatId, CancellationToken cancellationToken = default)
    {
        if (!IbmPcGeometryCatalog.TryFromFormatId(formatId, out var geometry)) throw IbmRawImageWriterExceptions.UnsupportedFormat(formatId);
        var expected = new RegularSectorGeometry(formatId, FatBootSectorLayout.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack);
        var source = image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) ? image : image.WithFormatId(formatId);
        if (image.BlockSize != expected.BlockSize || image.Cylinders != expected.Cylinders || image.Heads != expected.Heads || image.SectorsPerTrack != expected.SectorsPerTrack || image.BlockCount != expected.BlockCount) throw IbmRawImageWriterExceptions.GeometryMismatch(image.FormatId, formatId);
        return writer.WriteAsync(source, path, expected, cancellationToken);
    }
}
