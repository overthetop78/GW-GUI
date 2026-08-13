using GWGUI.MediaEngine.Containers.Commodore.D81;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Commodore;

namespace GWGUI.MediaEngine.Conversion.Commodore;

/// <summary>Convertit une capture SCP ou une image sectorielle Commodore 1581 en D81.</summary>
public sealed class D81ConversionService(CommodoreScpSectorImageReader scpReader, D81Reader reader, D81Writer writer)
{
    /// <summary>Indique si la cible est exactement une image Commodore 1581 D81.</summary>
    public static bool CanCreate(string formatId, string extension) => formatId.Equals(DiskImageFormatIds.Commodore1581, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.D81, StringComparison.OrdinalIgnoreCase);

    /// <summary>Reconstruit ou relit la source puis écrit son ordre logique D81.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string formatId, CancellationToken cancellationToken = default)
    {
        if (!CanCreate(formatId, Path.GetExtension(outputPath))) throw new InvalidDataException($"Format '{formatId}' cannot be written to '{Path.GetExtension(outputPath)}' as D81.");
        var image = Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase) ? await scpReader.ReadAsync(sourcePath, DiskImageFormatIds.Commodore1581, cancellationToken).ConfigureAwait(false) : await reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await writer.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
    }
}
