using GWGUI.MediaEngine.Containers.Dec.Rx02;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Dec;

namespace GWGUI.MediaEngine.Conversion.Dec;

/// <summary>Convertit les captures SCP et dumps RX02 vers un IMG physique DEC.</summary>
public sealed class DecRx02ConversionService(DecRx02ScpSectorImageReader scpReader, DecRx02Reader reader, DecRx02Writer writer)
{
    /// <summary>Indique si la cible est un dump IMG RX02.</summary>
    public static bool CanCreate(string formatId, string extension) => formatId.Equals(DiskImageFormatIds.DecRx02, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase);

    /// <summary>Reconstruit ou relit la source puis écrit son ordre physique RX02.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, CancellationToken cancellationToken = default)
    {
        var image = Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase) ? await scpReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false) : await reader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await writer.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
    }
}
