using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Containers.TeleDisk;
using GWGUI.MediaEngine.Containers.Ucsd.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Ucsd;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Ucsd;

/// <summary>Convertit les sources UCSD p-System en image IMG sectorielle brute.</summary>
public sealed class UcsdImgConversionService(IsoScpSectorImageReader scpReader, UcsdRawImageReader rawReader, Td0Reader td0Reader, LinearSectorImageWriter writer)
{
    /// <summary>Indique si la cible est une image UCSD IBM MFM brute.</summary>
    public static bool CanCreate(string formatId, string extension) => formatId.Equals(DiskImageFormatIds.UcsdIbmMfm, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase);

    /// <summary>Reconstruit ou relit la source, puis écrit les secteurs selon la géométrie UCSD explicite.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, CancellationToken cancellationToken = default)
    {
        var image = await ReadSourceAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await writer.WriteAsync(image.WithFormatId(DiskImageFormatIds.UcsdIbmMfm), outputPath, UcsdIbmMfmGeometry.SectorGeometry, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Sélectionne le Reader correspondant au conteneur source sans modifier son contenu sectoriel.</summary>
    private async Task<SectorImage> ReadSourceAsync(string sourcePath, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(sourcePath);
        if (extension.Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase)) return await scpReader.ReadAsync(sourcePath, DiskImageFormatIds.UcsdIbmMfm, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(DiskImageFileExtensions.Td0, StringComparison.OrdinalIgnoreCase)) return await td0Reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        return await rawReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
    }
}
