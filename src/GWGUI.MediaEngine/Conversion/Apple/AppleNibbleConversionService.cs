using System.Collections.Frozen;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding.Apple;
using GWGUI.MediaEngine.Reconstruction.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Apple;

/// <summary>Convertit une source Apple II représentable vers un conteneur NIB ou WOZ1.</summary>
public sealed class AppleNibbleConversionService
{
    private static readonly IReadOnlySet<string> OutputExtensions = new[] { DiskImageFileExtensions.Nib, DiskImageFileExtensions.Woz }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private readonly AppleDiskImageReader _appleReader;
    private readonly AppleScpSectorImageReader _scpReader;
    private readonly AppleDiskImageWriter _writer;

    /// <summary>Crée le service avec ses façades de lecture et d'écriture injectées.</summary>
    public AppleNibbleConversionService(AppleDiskImageReader appleReader, AppleScpSectorImageReader scpReader, AppleDiskImageWriter writer)
    {
        _appleReader = appleReader ?? throw new ArgumentNullException(nameof(appleReader));
        _scpReader = scpReader ?? throw new ArgumentNullException(nameof(scpReader));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    /// <summary>Indique si le format demandé peut produire un conteneur NIB ou WOZ.</summary>
    public static bool CanCreate(string formatId, string extension) => OutputExtensions.Contains(extension) && (formatId.Equals(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase) || AppleIITrackEncodingService.Supports(formatId));

    /// <summary>Indique que le format visible peut nécessiter un convertisseur externe si la source réelle n'est pas représentable.</summary>
    public static bool IsCatalogAliasTarget(string formatId, string extension) => OutputExtensions.Contains(extension) && (formatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIProDos140, StringComparison.OrdinalIgnoreCase));

    /// <summary>Lit, valide puis convertit la source sans créer la sortie avant la fin des validations.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, CancellationToken cancellationToken = default)
    {
        ValidateOutput(outputPath);
        var image = await ReadSourceAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        if (!image.FormatId.Equals(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase) && !AppleIITrackEncodingService.Supports(image.FormatId)) throw AppleNibbleConversionExceptions.InvalidSource(sourcePath, image.FormatId);
        await _writer.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reconnaît une signature SCP certaine, sinon délègue le contenu au routeur Apple.</summary>
    private async Task<SectorImage> ReadSourceAsync(string sourcePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, ScpFormatConstants.SignatureLength, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var signature = new byte[ScpFormatConstants.SignatureLength];
        var read = await stream.ReadAsync(signature, cancellationToken).ConfigureAwait(false);
        return read == signature.Length && signature.AsSpan().SequenceEqual(ScpFormatConstants.FileSignature) ? await _scpReader.ReadAsync(sourcePath, null, cancellationToken).ConfigureAwait(false) : await _appleReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Vérifie que l'extension de sortie correspond à un Writer disponible.</summary>
    private static void ValidateOutput(string outputPath)
    {
        var extension = Path.GetExtension(outputPath);
        if (!OutputExtensions.Contains(extension)) throw AppleNibbleConversionExceptions.UnsupportedOutput(outputPath, extension);
    }
}
