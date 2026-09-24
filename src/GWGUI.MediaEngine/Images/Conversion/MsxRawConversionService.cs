using GWGUI.MediaEngine.Images.Reading.Reconstruction.Iso;
using GWGUI.MediaEngine.Constants;

using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Conversion;

/// <summary>Convertit une capture SCP ou une image MSX brute en DSK.</summary>
public sealed class MsxRawConversionService(IsoScpSectorImageReader scpReader, MsxRawImageReader reader, MsxRawImageWriter writer)
{
    /// <summary>Indique si la sortie demandée est un profil MSX DSK explicite.</summary>
    public static bool CanCreate(string formatId, string extension) => MsxDiskGeometryCatalog.TryFromFormatId(formatId, out _) && extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase);

    /// <summary>Reconstruit ou relit la source puis écrit le DSK demandé.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string formatId, CancellationToken cancellationToken = default)
    {
        var image = Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase) ? await scpReader.ReadAsync(sourcePath, formatId, cancellationToken).ConfigureAwait(false) : await reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await writer.WriteAsync(image, outputPath, formatId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Écrit une image sectorielle MSX déjà reconstruite.</summary>
    public Task ConvertAsync(SectorImage image, string outputPath, string formatId, CancellationToken cancellationToken = default) => writer.WriteAsync(image, outputPath, formatId, cancellationToken);
}
