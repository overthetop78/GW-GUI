using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Apple.Raw;
using GWGUI.MediaEngine.Containers.Apple.TwoImg;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Apple;

/// <summary>Convertit les captures SCP et images Apple sectorielles vers D13, DO, DSK, PO ou 2MG.</summary>
public sealed class AppleSectorConversionService(AppleDiskImageReader imageReader, AppleScpSectorImageReader scpReader, AppleRawImageWriter rawWriter, TwoImgWriter twoImgWriter)
{
    /// <summary>Indique si le format et l'extension forment une cible sectorielle Apple cohérente.</summary>
    public static bool CanCreate(string formatId, string extension)
    {
        if (extension.Equals(DiskImageFileExtensions.TwoMg, StringComparison.OrdinalIgnoreCase)) return IsSupportedFormat(formatId);
        if (formatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase)) return extension.Equals(DiskImageFileExtensions.D13, StringComparison.OrdinalIgnoreCase);
        if (formatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase)) return extension.Equals(DiskImageFileExtensions.Do, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase);
        if (AppleRawImageWriter.IsProDos140(formatId)) return extension.Equals(DiskImageFileExtensions.Po, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Do, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase);
        return formatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.Po, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Reconstruit ou relit l'image source puis l'écrit dans le conteneur Apple demandé.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, string targetFormatId, CancellationToken cancellationToken = default)
    {
        var outputExtension = Path.GetExtension(outputPath);
        if (!CanCreate(targetFormatId, outputExtension)) throw new NotSupportedException($"Apple target '{targetFormatId}' with extension '{outputExtension}' is not supported.");
        var image = await ReadSourceAsync(sourcePath, targetFormatId, cancellationToken).ConfigureAwait(false);
        ValidateSource(image, targetFormatId);
        if (outputExtension.Equals(DiskImageFileExtensions.TwoMg, StringComparison.OrdinalIgnoreCase))
            await twoImgWriter.WriteAsync(image, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
        else
            await rawWriter.WriteAsync(image, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Lit une capture par son reconstructeur ou un conteneur par le routeur Apple commun.</summary>
    private async Task<SectorImage> ReadSourceAsync(string sourcePath, string targetFormatId, CancellationToken cancellationToken)
    {
        if (Path.GetExtension(sourcePath).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase)) return await scpReader.ReadAsync(sourcePath, targetFormatId, cancellationToken).ConfigureAwait(false);
        var bytes = await File.ReadAllBytesAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        return await imageReader.ReadAsync(bytes, Path.GetExtension(sourcePath), targetFormatId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Empêche de rebaptiser un système de fichiers en un autre sans conversion sémantique.</summary>
    private static void ValidateSource(SectorImage image, string targetFormatId)
    {
        var valid = targetFormatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase) && image.FormatId.Equals(DiskImageFormatIds.AppleIIDos32, StringComparison.OrdinalIgnoreCase)
            || targetFormatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase) && (image.FormatId.Equals(DiskImageFormatIds.AppleIIDos33, StringComparison.OrdinalIgnoreCase) || image.FormatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase))
            || targetFormatId.Equals(DiskImageFormatIds.AppleIIProDos140, StringComparison.OrdinalIgnoreCase) && (image.FormatId.Equals(DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase) || image.FormatId.Equals(DiskImageFormatIds.AppleIIProDos140, StringComparison.OrdinalIgnoreCase))
            || targetFormatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase) && (image.FormatId.Equals(DiskImageFormatIds.AppleIIProDos, StringComparison.OrdinalIgnoreCase) || image.FormatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase))
            || targetFormatId.Equals(DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase) && image.FormatId.Equals(DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase);
        if (!valid) throw new InvalidDataException($"Apple source format '{image.FormatId}' cannot be written as '{targetFormatId}' without changing its file system.");
    }

    /// <summary>Indique si l'identifiant appartient aux cinq profils sectoriels pris en charge.</summary>
    private static bool IsSupportedFormat(string formatId) => formatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase) || AppleRawImageWriter.IsProDos140(formatId) || formatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase);
}
