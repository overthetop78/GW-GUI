using GWGUI.MediaEngine.Containers.Coherent;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Commodore;

namespace GWGUI.MediaEngine.Conversion.Commodore;

/// <summary>Convertit les captures et dumps Commodore 900 COHERENT en BIN ou IMG.</summary>
public sealed class CoherentConversionService(CommodoreScpSectorImageReader scpReader, CoherentRawImageReader reader, CoherentRawImageWriter writer)
{
    /// <summary>Indique si la cible est un dump brut Commodore 900.</summary>
    public static bool CanCreate(string formatId, string extension) => formatId.Equals(DiskImageFormatIds.Commodore900Coherent, StringComparison.OrdinalIgnoreCase) && (extension.Equals(DiskImageFileExtensions.Bin, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase));

    /// <summary>Reconstruit ou relit la source, puis écrit son ordre logique zoné commun.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, CancellationToken cancellationToken = default)
    {
        var image = Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase) ? await scpReader.ReadAsync(sourcePath, DiskImageFormatIds.Commodore900Coherent, cancellationToken).ConfigureAwait(false) : await reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await writer.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
    }
}
