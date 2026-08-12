using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Containers.Msx.Raw;
using GWGUI.MediaEngine.Recognition.Msx;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>PrÃ©sÃ©lectionne les images brutes MSX-DOS Ã  partir de l'indice DSK, d'une demande MSX explicite et du BPB.</summary>
internal sealed class MsxImageRecognitionPolicy : ReaderBackedRecognitionPolicy
{
    /// <summary>CrÃ©e la politique en conservant le Reader responsable de la validation complÃ¨te du contenu.</summary>
    /// <param name="reader">Reader des images sectorielles MSX.</param>
    public MsxImageRecognitionPolicy(MsxRawImageReader reader) : base((context, cancellationToken) => reader.ReadAsync(context.Path, cancellationToken)) { }

    /// <summary>VÃ©rifie l'indice DSK puis accepte une demande <c>msx.*</c> explicite ou un BPB MSX valide.</summary>
    /// <param name="context">Contexte du fichier Ã  prÃ©sÃ©lectionner.</param>
    /// <param name="cancellationToken">Jeton d'annulation de la lecture du contenu partagÃ©.</param>
    /// <returns><see langword="true"/> lorsque le fichier DSK doit Ãªtre validÃ© par le Reader MSX.</returns>
    public override async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        if (!context.Extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase)) return false;
        return context.RequestedFormatId?.StartsWith(DiskImageFormatIds.MsxPrefix, StringComparison.OrdinalIgnoreCase) == true || MsxBootSectorProbe.LooksLikeMsx((await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false)).Span);
    }
}
