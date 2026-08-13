using GWGUI.MediaEngine.Containers.Atari.Msa;
using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Atari;

namespace GWGUI.MediaEngine.Conversion.Atari;

/// <summary>Convertit une capture SCP ou une image sectorielle Atari en ST ou MSA.</summary>
public sealed class AtariStConversionService(AtariScpSectorImageReader scpReader, AtariStReader stReader, MsaReader msaReader, AtariStWriter stWriter, MsaWriter msaWriter)
{
    /// <summary>Indique si la sortie demandée relève de la conversion Atari ST interne.</summary>
    public static bool CanCreate(string formatId, string extension) => formatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase) && (extension.Equals(DiskImageFileExtensions.St, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Msa, StringComparison.OrdinalIgnoreCase));

    /// <summary>Reconstruit le format demandé puis écrit les blocs logiques dans leur ordre.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string formatId, CancellationToken cancellationToken = default)
    {
        var sourceExtension = Path.GetExtension(sourcePath);
        var image = sourceExtension.Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase) ? await scpReader.ReadAsync(sourcePath, formatId, cancellationToken).ConfigureAwait(false) : sourceExtension.Equals(DiskImageFileExtensions.Msa, StringComparison.OrdinalIgnoreCase) ? await msaReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false) : await stReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        var transformed = AtariStGeometryTransformer.Transform(image, formatId, sourceExtension.Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase));
        if (Path.GetExtension(outputPath).Equals(DiskImageFileExtensions.Msa, StringComparison.OrdinalIgnoreCase)) await msaWriter.WriteAsync(transformed, outputPath, cancellationToken).ConfigureAwait(false);
        else await stWriter.WriteAsync(transformed, outputPath, cancellationToken).ConfigureAwait(false);
    }
}
