using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Commodore.D81;

/// <summary>Écrit les 3 200 blocs logiques d'une image Commodore 1581 dans un conteneur D81.</summary>
public sealed class D81Writer(LinearSectorImageWriter writer)
{
    /// <summary>Valide la géométrie logique D81 puis écrit chaque bloc sans remplissage implicite.</summary>
    public Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        var geometry = new RegularSectorGeometry(DiskImageFormatIds.Commodore1581, Commodore1581Geometry.LogicalBlockSize, Commodore1581Geometry.LogicalCylinderCount, Commodore1581Geometry.LogicalHeadCount, Commodore1581Geometry.LogicalBlocksPerTrack);
        return writer.WriteAsync(image, path, geometry, cancellationToken);
    }
}
