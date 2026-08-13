using GWGUI.MediaEngine.Containers.Msx.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Msx;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Msx;

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
