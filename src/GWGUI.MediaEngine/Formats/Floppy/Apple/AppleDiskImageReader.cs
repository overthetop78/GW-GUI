
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Formats.Floppy.DiskCopy;
using GWGUI.MediaEngine.Formats.Floppy.TwoImg;
using GWGUI.MediaEngine.Formats.Floppy.Woz;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.Apple;

/// <summary>Lit une image Apple en laissant le routeur distinguer les signatures certaines des simples indices de format.</summary>
public sealed class AppleDiskImageReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[]
    {
        DiskImageFormatIds.AppleIIAppleDos113,
        DiskImageFormatIds.AppleIIAppleDos140,
        DiskImageFormatIds.AppleIIDos32,
        DiskImageFormatIds.AppleIIDos33,
        DiskImageFormatIds.AppleIIGcr,
        DiskImageFormatIds.AppleIIProDos,
        DiskImageFormatIds.AppleIIProDos140,
        DiskImageFormatIds.AppleIIProDos800,
        DiskImageFormatIds.AppleIIRwts18,
        DiskImageFormatIds.AppleIIISos,
        DiskImageFormatIds.AppleLisaMacWorks,
        DiskImageFormatIds.AppleLisaOffice,
        DiskImageFormatIds.AppleLisaRaw,
        DiskImageFormatIds.AppleMacGcr,
        DiskImageFormatIds.AppleMacHfs,
        DiskImageFormatIds.AppleMacMfs,
        DiskImageFormatIds.Mac400,
        DiskImageFormatIds.Mac800,
        DiskImageFormatIds.Mac1440
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[]
    {
        DiskImageFileExtensions.TwoMg,
        DiskImageFileExtensions.D13,
        DiskImageFileExtensions.Dc42,
        DiskImageFileExtensions.Do,
        DiskImageFileExtensions.Dsk,
        DiskImageFileExtensions.Image,
        DiskImageFileExtensions.Img,
        DiskImageFileExtensions.Nib,
        DiskImageFileExtensions.Po,
        DiskImageFileExtensions.Woz
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> SupportedSignatures =
    [
        TwoImgFormat.SignatureBytes.ToArray(),
        WozFormat.Version1Signature.ToArray(),
        WozFormat.Version2Signature.ToArray()
    ];
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;

    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;

    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => SupportedSignatures;

    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;

    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;

    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;

    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportsFormatId(formatId);

    /// <summary>Charge une fois le fichier, puis valide et reconstruit son conteneur ou sa représentation Apple.</summary>
    /// <param name="path">Chemin du fichier Apple à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle entièrement validée.</returns>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return await ReadAsync(bytes, Path.GetExtension(path), null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Valide un contenu déjà chargé sans relire son fichier d'origine.</summary>
    /// <param name="bytes">Contenu complet de l'image.</param>
    /// <param name="extension">Extension servant uniquement d'indice aux formats sans signature.</param>
    /// <param name="requestedFormatId">Identifiant demandé, ou <see langword="null"/>.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler le routage.</param>
    /// <returns>Image sectorielle entièrement validée.</returns>
    public Task<SectorImage> ReadAsync(ReadOnlyMemory<byte> bytes, string extension, string? requestedFormatId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(AppleContainerRouter.Read(bytes.ToArray(), extension, requestedFormatId));
    }

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportsFormatId(context.RequestedFormatId)) return false;

        try
        {
            var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
            _ = AppleContainerRouter.Read(bytes.ToArray(), context.Extension, context.RequestedFormatId);
            return true;
        }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        {
            return false;
        }
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        var container = bytes.ToArray();
        if (DiskCopyReader.HasPrivateWord(container))
        {
            var diskCopy = DiskCopyReader.ReadDetailed(container);
            return MediaImageDocumentFactory.CreateFloppySector(
                context.Source,
                diskCopy.Image,
                DiskCopyMetadataFunctions.Create(diskCopy));
        }

        var image = AppleContainerRouter.Read(container, context.Extension, context.RequestedFormatId);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, image);
    }

    private static bool SupportsFormatId(string formatId)
        => SupportedFormatIds.Contains(formatId) || AppleDiskImageFormatFamilies.Contains(formatId);
}
