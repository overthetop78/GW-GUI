using GWGUI.MediaEngine.Containers.Apple.DiskCopy;
using GWGUI.MediaEngine.Containers.Apple.Nib;
using GWGUI.MediaEngine.Containers.Apple.TwoImg;
using GWGUI.MediaEngine.Containers.Apple.Woz;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Recognition.Apple;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Présélectionne une signature certaine, un indice d'extension ou une famille Apple explicitement demandée, puis laisse le Reader valider entièrement le contenu.</summary>
/// <param name="reader">Lecteur public chargé de valider et reconstruire le candidat Apple.</param>
internal sealed class AppleImageRecognitionPolicy(AppleDiskImageReader reader) : ReaderBackedRecognitionPolicy(async (context, cancellationToken) => await reader.ReadAsync(await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false), context.Extension, context.RequestedFormatId, cancellationToken).ConfigureAwait(false))
{
    /// <summary>Extensions servant uniquement d'indices pour les représentations Apple sans signature.</summary>
    private static readonly IReadOnlySet<string> ExtensionHints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        DiskImageFileExtensions.D13,
        DiskImageFileExtensions.Do,
        DiskImageFileExtensions.Po,
        DiskImageFileExtensions.Nib,
        DiskImageFileExtensions.Dsk,
        DiskImageFileExtensions.Img,
        DiskImageFileExtensions.Image,
        DiskImageFileExtensions.Dc42
    };

    /// <summary>Recherche d'abord les signatures 2IMG, DiskCopy et WOZ, puis examine séparément les indices d'extension et la famille explicitement demandée.</summary>
    /// <param name="context">Contexte partagé du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture du contenu.</param>
    /// <returns><see langword="true"/> lorsqu'une signature, un indice ou une demande explicite présélectionne le Reader ; sinon <see langword="false"/>.</returns>
    public override async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        if (bytes.Span.StartsWith(TwoImgFormat.SignatureBytes) ||
            DiskCopyReader.HasPrivateWord(bytes.Span) ||
            bytes.Span.StartsWith(WozFormat.Version1Signature) ||
            bytes.Span.StartsWith(WozFormat.Version2Signature))
            return true;

        if (context.Extension.Equals(DiskImageFileExtensions.Nib, StringComparison.OrdinalIgnoreCase)) return bytes.Length >= NibLayout.TrackLengthBytes;
        if (!ExtensionHints.Contains(context.Extension)) return false;
        if (context.Extension is DiskImageFileExtensions.Dsk or DiskImageFileExtensions.Img) return AppleDiskImageFormatFamilies.Contains(context.RequestedFormatId) || AppleRawImageProbe.LooksLikeAppleImage(context.Extension, bytes, context.RequestedFormatId);
        return true;
    }

    /// <summary>Indique si l'identifiant demandé appartient à une famille Apple reconnue.</summary>
    /// <param name="formatId">Identifiant demandé, ou <see langword="null"/>.</param>
    /// <returns><see langword="true"/> pour une famille Apple II, Apple III, Lisa ou Macintosh.</returns>
}
