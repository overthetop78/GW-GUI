using GWGUI.MediaEngine.Containers.Apple.DiskCopy;
using GWGUI.MediaEngine.Containers.Apple.Nib;
using GWGUI.MediaEngine.Containers.Apple.TwoImg;
using GWGUI.MediaEngine.Containers.Apple.Woz;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Recognition.Apple;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Reconnaît les conteneurs Apple signés et présélectionne les représentations Apple brutes.</summary>
/// <param name="reader">Lecteur public chargé de valider et reconstruire le candidat Apple.</param>
internal sealed class AppleImageRecognitionPolicy(AppleDiskImageReader reader) : ReaderBackedRecognitionPolicy(async (context, cancellationToken) => await reader.ReadAsync(await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false), context.Extension, context.RequestedFormatId, cancellationToken).ConfigureAwait(false))
{
    /// <summary>Extensions servant uniquement d'indices pour les représentations Apple sans signature.</summary>
    private static readonly HashSet<string> RawHints = new(StringComparer.OrdinalIgnoreCase)
    {
        DiskImageFileExtensions.Do,
        DiskImageFileExtensions.Po,
        DiskImageFileExtensions.D13,
        DiskImageFileExtensions.Nib
    };

    /// <summary>Recherche d'abord les marqueurs 2IMG, DiskCopy et WOZ, puis examine les indices des formats bruts.</summary>
    /// <param name="context">Contexte partagé du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture du contenu.</param>
    /// <returns><see langword="true"/> pour un conteneur signé ou un candidat brut Apple ; sinon <see langword="false"/>.</returns>
    public override async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        if (bytes.Span.StartsWith(TwoImgFormat.SignatureBytes) ||
            DiskCopyReader.HasPrivateWord(bytes.Span) ||
            bytes.Span.StartsWith(WozFormat.Version1Signature) ||
            bytes.Span.StartsWith(WozFormat.Version2Signature))
            return true;

        if (context.Extension.Equals(DiskImageFileExtensions.Nib, StringComparison.OrdinalIgnoreCase)) return bytes.Length >= NibLayout.TrackLengthBytes;
        if (RawHints.Contains(context.Extension)) return true;
        if (context.Extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase))
            return IsRequestedAppleFormat(context.RequestedFormatId) || AppleRawImageProbe.LooksLikeAppleImage(context.Extension, bytes, context.RequestedFormatId);
        if (context.Extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase))
            return context.RequestedFormatId?.StartsWith(DiskImageFormatIds.MacPrefix, StringComparison.OrdinalIgnoreCase) == true ||
                   context.RequestedFormatId?.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) == true ||
                   AppleRawImageProbe.LooksLikeAppleImage(context.Extension, bytes, context.RequestedFormatId);
        return false;
    }

    /// <summary>Indique si l'identifiant demandé appartient à une famille Apple reconnue.</summary>
    /// <param name="formatId">Identifiant demandé, ou <see langword="null"/>.</param>
    /// <returns><see langword="true"/> pour une famille Apple II, Apple III, Lisa ou Macintosh.</returns>
    private static bool IsRequestedAppleFormat(string? formatId) =>
        formatId?.StartsWith(DiskImageFormatIds.AppleIIPrefix, StringComparison.OrdinalIgnoreCase) == true ||
        formatId?.StartsWith(DiskImageFormatIds.AppleIIIPrefix, StringComparison.OrdinalIgnoreCase) == true ||
        formatId?.StartsWith(DiskImageFormatIds.AppleLisaPrefix, StringComparison.OrdinalIgnoreCase) == true ||
        formatId?.StartsWith(DiskImageFormatIds.AppleMacPrefix, StringComparison.OrdinalIgnoreCase) == true;
}
