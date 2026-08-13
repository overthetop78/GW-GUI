using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Apple.DiskCopy;
using GWGUI.MediaEngine.Containers.Apple.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Apple;

/// <summary>Convertit les images Macintosh entre SCP, brut et DiskCopy 4.2.</summary>
public sealed class MacintoshConversionService(AppleDiskImageReader reader, AppleScpSectorImageReader scpReader, MacintoshRawImageWriter rawWriter, DiskCopyWriter diskCopyWriter)
{
    /// <summary>Indique si la cible est un IMG Macintosh ou un conteneur DiskCopy.</summary>
    public static bool CanCreate(string formatId, string extension) => IsMacintosh(formatId) && (extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Image, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Dc42, StringComparison.OrdinalIgnoreCase));

    /// <summary>Lit la source une seule fois, préserve l'en-tête DiskCopy disponible et écrit la cible.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string targetFormatId, CancellationToken cancellationToken = default)
    {
        var sourceBytes = await File.ReadAllBytesAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        DiskCopyImage? diskCopy = DiskCopyReader.HasPrivateWord(sourceBytes) ? DiskCopyReader.ReadDetailed(sourceBytes) : null;
        SectorImage image;
        if (Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase)) image = await scpReader.ReadAsync(sourcePath, targetFormatId, cancellationToken).ConfigureAwait(false);
        else image = diskCopy?.Image ?? await reader.ReadAsync(sourceBytes, Path.GetExtension(sourcePath), targetFormatId, cancellationToken).ConfigureAwait(false);
        var extension = Path.GetExtension(outputPath);
        if (extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase)) await rawWriter.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
        else await diskCopyWriter.WriteAsync(image, outputPath, diskCopy, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reconnaît les identifiants Macintosh bruts et catalogués.</summary>
    private static bool IsMacintosh(string formatId) => formatId.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase);
}
