using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Acorn;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Containers.Acorn.BbcDfs;

/// <summary>Lit les images BBC DFS SSD et DSD dans leur ordre cylindre, face et secteur à base zéro.</summary>
public sealed class BbcDfsReader
{
    /// <summary>Charge l'image, sélectionne SSD ou DSD depuis l'extension et exige une capacité exacte de 40 ou 80 cylindres.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(path);
        var kind = extension.Equals(DiskImageFileExtensions.Ssd, StringComparison.OrdinalIgnoreCase) ? BbcDfsContainerKind.Ssd : extension.Equals(DiskImageFileExtensions.Dsd, StringComparison.OrdinalIgnoreCase) ? BbcDfsContainerKind.Dsd : throw BbcDfsExceptions.UnknownExtension(extension);
        var heads = kind == BbcDfsContainerKind.Ssd ? DiskGeometryConstants.SingleSidedHeadCount : DiskGeometryConstants.DoubleSidedHeadCount;
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (data.Length == 0 || data.Length % (BbcDfsGeometry.TrackSize * heads) != 0) throw BbcDfsExceptions.IncompleteTrack(data.Length, heads, BbcDfsGeometry.TrackSize);
        var cylinders = data.Length / (BbcDfsGeometry.TrackSize * heads);
        var geometry = BbcDfsGeometry.Find(heads, data.Length) ?? throw BbcDfsExceptions.UnsupportedCylinderCount(data.Length, cylinders, heads);
        var linear = new LinearSectorImageGeometry(BbcDfsGeometry.SectorSize, geometry.Cylinders, geometry.Heads, BbcDfsGeometry.SectorsPerTrack, SectorNumbering.ZeroBased);
        return LinearSectorImageBuilder.Create(data, geometry.FormatId, linear, cancellationToken);
    }
}
