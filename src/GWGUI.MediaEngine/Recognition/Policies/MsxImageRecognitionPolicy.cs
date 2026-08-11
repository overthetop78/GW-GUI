using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Recognition.Msx;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Présélectionne les images brutes MSX-DOS à partir de l'indice DSK, d'une demande MSX explicite et du BPB.</summary>
internal sealed class MsxImageRecognitionPolicy : ReaderBackedRecognitionPolicy
{
    /// <summary>Crée la politique en conservant le Reader responsable de la validation complète du contenu.</summary>
    /// <param name="reader">Reader des images sectorielles MSX.</param>
    public MsxImageRecognitionPolicy(MsxImageReader reader) : base(reader.ReadAsync) { }

    /// <summary>Vérifie l'indice DSK puis accepte une demande <c>msx.*</c> explicite ou un BPB MSX valide.</summary>
    /// <param name="context">Contexte du fichier à présélectionner.</param>
    /// <param name="cancellationToken">Jeton d'annulation de la lecture du contenu partagé.</param>
    /// <returns><see langword="true"/> lorsque le fichier DSK doit être validé par le Reader MSX.</returns>
    public override async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        if (!context.Extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase)) return false;
        return context.RequestedFormatId?.StartsWith(DiskImageFormatIds.MsxPrefix, StringComparison.OrdinalIgnoreCase) == true || MsxBootSectorProbe.LooksLikeMsx((await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false)).Span);
    }
}
