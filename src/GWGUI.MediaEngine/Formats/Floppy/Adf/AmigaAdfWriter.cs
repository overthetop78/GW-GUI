using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.Constants;

using GWGUI.MediaEngine.Formats.Floppy.Raw;

using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.Adf;

/// <summary>Écrit les images sectorielles Amiga ADF DD et HD.</summary>
public sealed class AmigaAdfWriter(LinearSectorImageWriter writer)
{
    /// <summary>Crée un Writer ADF utilisant le Writer sectoriel linéaire commun.</summary>
    public AmigaAdfWriter() : this(new LinearSectorImageWriter()) { }

    /// <summary>Écrit l'image Amiga après validation stricte de sa géométrie.</summary>
    public Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default) => writer.WriteAsync(image, path, Geometry(image.FormatId), cancellationToken);

    private static RegularSectorGeometry Geometry(string formatId)
    {
        if (formatId.Equals(DiskImageFormatIds.AmigaDos, StringComparison.OrdinalIgnoreCase)) return AmigaAdfGeometry.DoubleDensity;
        if (formatId.Equals(DiskImageFormatIds.AmigaDosHighDensity, StringComparison.OrdinalIgnoreCase)) return AmigaAdfGeometry.HighDensity;
        throw AmigaAdfWriterExceptions.UnsupportedFormat(formatId);
    }
}
