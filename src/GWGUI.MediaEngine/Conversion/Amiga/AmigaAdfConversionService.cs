using GWGUI.MediaEngine.Containers.Adf;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Amiga;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Amiga;

/// <summary>Convertit une capture SCP ou une image sectorielle Amiga en ADF DD ou HD.</summary>
public sealed class AmigaAdfConversionService(AmigaScpSectorImageReader scpReader, AdfReader reader, AmigaAdfWriter writer)
{
    /// <summary>Indique si la sortie demandée est un ADF Amiga géré en interne.</summary>
    public static bool CanCreate(string formatId, string extension) => (formatId.Equals(DiskImageFormatIds.AmigaDos, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AmigaDosHighDensity, StringComparison.OrdinalIgnoreCase)) && extension.Equals(DiskImageFileExtensions.Adf, StringComparison.OrdinalIgnoreCase);

    /// <summary>Reconstruit ou relit la source puis écrit l'ADF demandé.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string formatId, CancellationToken cancellationToken = default)
    {
        var image = Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase) ? await scpReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false) : await reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await ConvertAsync(image, outputPath, formatId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reconstruit la source et conserve automatiquement sa géométrie Amiga DD ou HD.</summary>
    public async Task ConvertDetectedAsync(string sourcePath, string outputPath, CancellationToken cancellationToken = default)
    {
        var image = Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase)
            ? await scpReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false)
            : await reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        if (!image.FormatId.Equals(DiskImageFormatIds.AmigaDos, StringComparison.OrdinalIgnoreCase) &&
            !image.FormatId.Equals(DiskImageFormatIds.AmigaDosHighDensity, StringComparison.OrdinalIgnoreCase))
            throw AmigaAdfWriterExceptions.FormatMismatch(image.FormatId, DiskImageFormatIds.AmigaDos);
        await writer.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Valide et écrit directement une image sectorielle déjà reconstruite.</summary>
    public Task ConvertAsync(SectorImage image, string outputPath, string formatId, CancellationToken cancellationToken = default)
    {
        if (!image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase)) throw AmigaAdfWriterExceptions.FormatMismatch(image.FormatId, formatId);
        return writer.WriteAsync(image, outputPath, cancellationToken);
    }
}
