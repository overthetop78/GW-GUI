using GWGUI.MediaEngine.Containers.Atari.Atr;
using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Atari;

namespace GWGUI.MediaEngine.Conversion.Atari;

/// <summary>Reconstruit une capture SCP Atari dans le format sectoriel attendu par l'émulateur.</summary>
public sealed class AtariScpRuntimeConversionService(
    AtariScpSectorImageReader reader,
    AtrWriter atrWriter,
    AtariStWriter stWriter)
{
    public async Task ConvertToAtrAsync(string sourcePath, string outputPath,
        CancellationToken cancellationToken = default)
    {
        var image = await reader.ReadAsync(sourcePath, null, cancellationToken).ConfigureAwait(false);
        if (!image.FormatId.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"The SCP image '{sourcePath}' is not an Atari 8-bit disk.");
        await atrWriter.WriteAsync(image, outputPath, image.FormatId, cancellationToken).ConfigureAwait(false);
    }

    public async Task ConvertToStAsync(string sourcePath, string outputPath,
        CancellationToken cancellationToken = default)
    {
        var image = await reader.ReadAsync(sourcePath, null, cancellationToken).ConfigureAwait(false);
        if (!image.FormatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"The SCP image '{sourcePath}' is not an Atari ST disk.");
        await stWriter.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
    }
}