using GWGUI.MediaEngine.Images.Reading.Reconstruction.Iso;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Images.Formats.Floppy.Adf;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Conversion.Acorn;

/// <summary>Convertit une capture SCP ou une image ADF Acorn en ADF 800 Kio.</summary>
public sealed class AcornAdfConversionService(IsoScpSectorImageReader scpReader, AdfReader reader, AcornAdfWriter writer)
{
    /// <summary>Annonce ADF pour les géométries Acorn et ADS/ADM/ADL pour les géométries BBC correspondantes.</summary>
    public static bool CanCreate(string formatId, string extension) => AcornAdfGeometry.All.Any(geometry => geometry.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase))
        && (extension.Equals(DiskImageFileExtensions.Adf, StringComparison.OrdinalIgnoreCase) || (formatId, extension.ToLowerInvariant()) is (DiskImageFormatIds.AcornAdfs160, DiskImageFileExtensions.Ads) or (DiskImageFormatIds.AcornAdfs320, DiskImageFileExtensions.Adm) or (DiskImageFormatIds.AcornAdfs640, DiskImageFileExtensions.Adl));

    /// <summary>Reconstruit ou relit la source puis écrit l'ADF Acorn.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string formatId, CancellationToken cancellationToken = default)
    {
        var image = await MediaConversionService.ReadSectorSourceAsync(sourcePath, () => scpReader.ReadAsync(sourcePath, formatId, cancellationToken), () => reader.ReadAsync(sourcePath, cancellationToken)).ConfigureAwait(false);
        await writer.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Écrit une image sectorielle Acorn déjà reconstruite.</summary>
    public Task ConvertAsync(SectorImage image, string outputPath, CancellationToken cancellationToken = default) => writer.WriteAsync(image, outputPath, cancellationToken);
}
