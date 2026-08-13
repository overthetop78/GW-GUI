using GWGUI.MediaEngine.Containers.Ibm.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Ibm;

/// <summary>Convertit une capture SCP ou une image IBM brute en IMA ou IMG.</summary>
public sealed class IbmRawConversionService(IsoScpSectorImageReader scpReader, IbmRawImageReader reader, IbmRawImageWriter writer)
{
    /// <summary>Indique si la sortie demandée est un profil IBM brut explicite.</summary>
    public static bool CanCreate(string formatId, string extension) => !formatId.Equals(DiskImageFormatIds.IbmScan, StringComparison.OrdinalIgnoreCase) && Geometries.Ibm.IbmPcGeometryCatalog.TryFromFormatId(formatId, out _) && (extension.Equals(DiskImageFileExtensions.Ima, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase));

    /// <summary>Reconstruit ou relit la source puis écrit le profil IBM demandé.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string formatId, CancellationToken cancellationToken = default)
    {
        var image = Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase) ? await scpReader.ReadAsync(sourcePath, formatId, cancellationToken).ConfigureAwait(false) : await reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await ConvertAsync(image, outputPath, formatId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Écrit une image sectorielle dont la géométrie correspond au profil cible.</summary>
    public Task ConvertAsync(SectorImage image, string outputPath, string formatId, CancellationToken cancellationToken = default) => writer.WriteAsync(image, outputPath, formatId, cancellationToken);
}
