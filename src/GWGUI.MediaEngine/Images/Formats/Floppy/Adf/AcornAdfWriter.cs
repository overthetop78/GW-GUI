
using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Adf;

/// <summary>Écrit les géométries ADFS Acorn dans leur ordre sectoriel, sans rembourrage ambigu.</summary>
public sealed class AcornAdfWriter(LinearSectorImageWriter writer)
{
    /// <summary>Crée un Writer Acorn ADF utilisant le Writer linéaire commun.</summary>
    public AcornAdfWriter() : this(new LinearSectorImageWriter()) { }

    /// <summary>Écrit les blocs validés selon la géométrie ADFS annoncée par l'image.</summary>
    public Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default) => writer.WriteAsync(image, path, AcornAdfGeometry.All.Single(geometry => geometry.FormatId.Equals(image.FormatId, StringComparison.OrdinalIgnoreCase)), cancellationToken);
}
