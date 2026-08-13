using GWGUI.MediaEngine.Containers.Atari.Atr;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Atari;

namespace GWGUI.MediaEngine.Conversion.Atari;

/// <summary>Convertit une capture SCP ou un conteneur ATR vers un profil ATR Atari 8-bit.</summary>
public sealed class AtrConversionService(AtariScpSectorImageReader scpReader, AtrReader reader, AtrWriter writer)
{
    /// <summary>Indique si la cible est l'un des trois profils ATR catalogués.</summary>
    public static bool CanCreate(string formatId, string extension) => AtrFormatCatalog.TryGet(formatId, out _) && extension.Equals(DiskImageFileExtensions.Atr, StringComparison.OrdinalIgnoreCase);

    /// <summary>Reconstruit ou relit la source puis écrit un conteneur ATR complet.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string formatId, CancellationToken cancellationToken = default)
    {
        var image = Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase) ? await scpReader.ReadAsync(sourcePath, formatId, cancellationToken).ConfigureAwait(false) : await reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await writer.WriteAsync(image, outputPath, formatId, cancellationToken).ConfigureAwait(false);
    }
}
