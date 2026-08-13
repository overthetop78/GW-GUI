using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Acorn;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Acorn.BbcDfs;

/// <summary>Écrit les images BBC DFS SSD et DSD en ordre cylindre, face, secteur.</summary>
public sealed class BbcDfsImageWriter(LinearSectorImageWriter writer)
{
    /// <summary>Crée un Writer BBC DFS utilisant le Writer linéaire commun.</summary>
    public BbcDfsImageWriter() : this(new LinearSectorImageWriter()) { }

    /// <summary>Valide le type de conteneur et écrit tous les secteurs du profil demandé.</summary>
    public Task WriteAsync(SectorImage image, string path, string formatId, CancellationToken cancellationToken = default)
    {
        var geometry = BbcDfsGeometry.FindByFormatId(formatId);
        var extension = Path.GetExtension(path);
        var expectedExtension = geometry?.Heads == 1 ? DiskImageFileExtensions.Ssd : DiskImageFileExtensions.Dsd;
        if (geometry is null || !extension.Equals(expectedExtension, StringComparison.OrdinalIgnoreCase)) throw BbcDfsImageWriterExceptions.UnsupportedTarget(formatId, extension);
        var expected = new RegularSectorGeometry(formatId, BbcDfsGeometry.SectorSize, geometry.Cylinders, geometry.Heads, BbcDfsGeometry.SectorsPerTrack);
        return writer.WriteAsync(image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) ? image : image.WithFormatId(formatId), path, expected, cancellationToken);
    }
}
