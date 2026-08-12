using System.Collections.Frozen;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Apple;

/// <summary>Convertit une source Apple II RWTS18 reconnue vers un conteneur NIB ou WOZ1.</summary>
public sealed class AppleRwts18ConversionService
{
    private static readonly IReadOnlySet<string> OutputExtensions = new[] { DiskImageFileExtensions.Nib, DiskImageFileExtensions.Woz }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private readonly AppleDiskImageReader _appleReader;
    private readonly AppleScpSectorImageReader _scpReader;
    private readonly AppleDiskImageWriter _writer;

    /// <summary>CrÃ©e le service avec ses faÃ§ades de lecture et d'Ã©criture injectÃ©es.</summary>
    /// <param name="appleReader">Lecteur des conteneurs Apple.</param>
    /// <param name="scpReader">Reconstructeur RWTS18 depuis SCP.</param>
    /// <param name="writer">FaÃ§ade d'Ã©criture NIB et WOZ1.</param>
    public AppleRwts18ConversionService(AppleDiskImageReader appleReader, AppleScpSectorImageReader scpReader, AppleDiskImageWriter writer)
    {
        _appleReader = appleReader ?? throw new ArgumentNullException(nameof(appleReader));
        _scpReader = scpReader ?? throw new ArgumentNullException(nameof(scpReader));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    /// <summary>Indique si le format RWTS18 peut produire l'extension demandÃ©e.</summary>
    public static bool CanCreate(string formatId, string extension) => formatId.Equals(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase) && OutputExtensions.Contains(extension);

    /// <summary>Lit, valide puis convertit une source RWTS18 sans crÃ©er la sortie avant la fin des validations.</summary>
    public async Task ConvertAsync(string sourcePath, string outputPath, CancellationToken cancellationToken = default)
    {
        ValidateOutput(outputPath);
        var image = await ReadSourceAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        ValidateSource(image, sourcePath);
        await WriteOutputAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>ReconnaÃ®t une signature SCP certaine, sinon dÃ©lÃ¨gue le contenu au routeur Apple.</summary>
    private async Task<SectorImage> ReadSourceAsync(string sourcePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, ScpFormatConstants.SignatureLength, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var signature = new byte[ScpFormatConstants.SignatureLength];
        var read = await stream.ReadAsync(signature, cancellationToken).ConfigureAwait(false);
        return read == signature.Length && signature.AsSpan().SequenceEqual(ScpFormatConstants.FileSignature) ? await _scpReader.ReadAsync(sourcePath, DiskImageFormatIds.AppleIIRwts18, cancellationToken).ConfigureAwait(false) : await _appleReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>VÃ©rifie que la lecture a produit une image RWTS18.</summary>
    private static void ValidateSource(SectorImage image, string sourcePath)
    {
        if (!image.FormatId.Equals(DiskImageFormatIds.AppleIIRwts18, StringComparison.OrdinalIgnoreCase)) throw AppleRwts18ConversionExceptions.InvalidSource(sourcePath, image.FormatId);
    }

    /// <summary>VÃ©rifie que l'extension de sortie correspond Ã  un Writer disponible.</summary>
    private static void ValidateOutput(string outputPath)
    {
        var extension = Path.GetExtension(outputPath);
        if (!OutputExtensions.Contains(extension)) throw AppleRwts18ConversionExceptions.UnsupportedOutput(outputPath, extension);
    }

    /// <summary>Ã‰crit l'image validÃ©e avec la faÃ§ade injectÃ©e.</summary>
    private Task WriteOutputAsync(SectorImage image, string outputPath, CancellationToken cancellationToken) => _writer.WriteAsync(image, outputPath, cancellationToken);
}

