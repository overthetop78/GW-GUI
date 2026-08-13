using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Apple.DiskCopy;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Apple;

/// <summary>Convertit les images Apple Lisa Office et MacWorks vers DiskCopy 4.2.</summary>
public sealed class LisaConversionService(AppleDiskImageReader reader, AppleScpSectorImageReader scpReader, DiskCopyWriter writer)
{
    /// <summary>Indique si la cible est une image Lisa dans un conteneur DiskCopy.</summary>
    public static bool CanCreate(string formatId, string extension) => formatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) && (extension.Equals(DiskImageFileExtensions.Image, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Dc42, StringComparison.OrdinalIgnoreCase));

    /// <summary>Lit une source Apple, conserve les métadonnées DiskCopy disponibles et écrit les données ainsi que leurs tags.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string targetFormatId, CancellationToken cancellationToken = default)
    {
        var sourceExtension = Path.GetExtension(sourcePath);
        DiskCopyImage? diskCopy = null;
        SectorImage image;
        if (sourceExtension.Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase)) image = await scpReader.ReadAsync(sourcePath, targetFormatId, cancellationToken).ConfigureAwait(false);
        else
        {
            var sourceBytes = await File.ReadAllBytesAsync(sourcePath, cancellationToken).ConfigureAwait(false);
            diskCopy = DiskCopyReader.HasPrivateWord(sourceBytes) ? DiskCopyReader.ReadDetailed(sourceBytes) : null;
            image = diskCopy?.Image ?? await reader.ReadAsync(sourceBytes, sourceExtension, targetFormatId, cancellationToken).ConfigureAwait(false);
        }
        if (!image.FormatId.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException($"L'image source n'est pas une image Apple Lisa reconnue ({image.FormatId}).");
        await writer.WriteAsync(image, outputPath, diskCopy, cancellationToken).ConfigureAwait(false);
    }
}
