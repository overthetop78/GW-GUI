using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Recognition;

namespace GWGUI.MediaEngine.Conversion.Scp;

/// <summary>Reconnaît une image sectorielle sur disque puis reconstruit sa représentation SCP.</summary>
public sealed class SectorImageScpFileConversionService(DiskImageRecognitionRegistry recognition, SectorImageScpConversionService conversion)
{
    public static bool CanCreate(string formatId, string extension) =>
        formatId.Equals(DiskImageFormatIds.RawScp, StringComparison.OrdinalIgnoreCase) &&
        extension.Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase);

    public async Task ConvertAsync(string sourcePath, string outputPath, CancellationToken cancellationToken = default)
    {
        var image = await recognition.ReadAsync(sourcePath, null, cancellationToken).ConfigureAwait(false);
        await conversion.ConvertAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
    }
}
