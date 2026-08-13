using GWGUI.MediaEngine.Containers.Adf;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Acorn;

/// <summary>Convertit une capture SCP ou une image ADF Acorn en ADF 800 Kio.</summary>
public sealed class AcornAdfConversionService(IsoScpSectorImageReader scpReader, AdfReader reader, AcornAdfWriter writer)
{
    /// <summary>Indique si la sortie demandée est explicitement un ADF Acorn 800 Kio.</summary>
    public static bool CanCreate(string formatId, string extension) => formatId.Equals(DiskImageFormatIds.AcornAdfs800, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.Adf, StringComparison.OrdinalIgnoreCase);

    /// <summary>Reconstruit ou relit la source puis écrit l'ADF Acorn.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string formatId, CancellationToken cancellationToken = default)
    {
        var image = Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase) ? await scpReader.ReadAsync(sourcePath, formatId, cancellationToken).ConfigureAwait(false) : await reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await writer.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Écrit une image sectorielle Acorn déjà reconstruite.</summary>
    public Task ConvertAsync(SectorImage image, string outputPath, CancellationToken cancellationToken = default) => writer.WriteAsync(image, outputPath, cancellationToken);
}
