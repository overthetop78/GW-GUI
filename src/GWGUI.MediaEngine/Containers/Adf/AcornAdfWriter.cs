using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Geometries.Acorn;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Adf;

/// <summary>Écrit une image sectorielle Acorn ADFS 800 Kio sans rembourrage ambigu.</summary>
public sealed class AcornAdfWriter(LinearSectorImageWriter writer)
{
    /// <summary>Crée un Writer Acorn ADF utilisant le Writer linéaire commun.</summary>
    public AcornAdfWriter() : this(new LinearSectorImageWriter()) { }

    /// <summary>Écrit exactement les 800 Kio de blocs ADFS validés.</summary>
    public Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default) => writer.WriteAsync(image, path, AcornAdfGeometry.Geometry, cancellationToken);
}
