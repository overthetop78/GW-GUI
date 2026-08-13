using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Amiga;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Adf;

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
