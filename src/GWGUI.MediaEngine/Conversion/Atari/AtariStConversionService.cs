using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Atari;

namespace GWGUI.MediaEngine.Conversion.Atari;

/// <summary>Convertit une capture SCP ou une image ST en image sectorielle Atari ST brute.</summary>
public sealed class AtariStConversionService(AtariScpSectorImageReader scpReader, AtariStReader reader, AtariStWriter writer)
{
    /// <summary>Indique si la sortie demandée relève de la conversion Atari ST interne.</summary>
    public static bool CanCreate(string formatId, string extension) =>
        formatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.St, StringComparison.OrdinalIgnoreCase);

    /// <summary>Reconstruit le format demandé puis écrit les blocs logiques dans leur ordre.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string formatId, CancellationToken cancellationToken = default)
    {
        var image = Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase)
            ? await scpReader.ReadAsync(sourcePath, formatId, cancellationToken).ConfigureAwait(false)
            : await reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await writer.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
    }
}
