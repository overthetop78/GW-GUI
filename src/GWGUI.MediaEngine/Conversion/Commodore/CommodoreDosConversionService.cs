using GWGUI.MediaEngine.Containers.Commodore;
using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Containers.Commodore.D71;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Commodore;

namespace GWGUI.MediaEngine.Conversion.Commodore;

/// <summary>Convertit les captures SCP et conteneurs Commodore DOS vers D64 ou D71.</summary>
public sealed class CommodoreDosConversionService(CommodoreScpSectorImageReader scpReader, D64Reader d64Reader, D71Reader d71Reader, CommodoreDosContainerWriter writer)
{
    /// <summary>Indique si le format et l'extension forment une cible D64 ou D71 cohérente.</summary>
    public static bool CanCreate(string formatId, string extension) => formatId.Equals(DiskImageFormatIds.Commodore1541, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.D64, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Commodore1571, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.D71, StringComparison.OrdinalIgnoreCase);

    /// <summary>Reconstruit ou relit la source et conserve sa carte d'erreurs lorsqu'elle existe entièrement.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string formatId, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(sourcePath);
        var image = extension.Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase) ? await scpReader.ReadAsync(sourcePath, formatId, cancellationToken).ConfigureAwait(false) : extension.Equals(DiskImageFileExtensions.D71, StringComparison.OrdinalIgnoreCase) ? await d71Reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false) : await d64Reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        if (!image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException($"Commodore source format '{image.FormatId}' cannot be written as '{formatId}' without a geometry transformation.");
        var errorMapMode = image.AvailableBlocks.All(block => block.DiagnosticCode.HasValue) ? CommodoreDosErrorMapMode.Preserve : CommodoreDosErrorMapMode.None;
        await writer.WriteAsync(image, outputPath, errorMapMode, cancellationToken).ConfigureAwait(false);
    }
}
