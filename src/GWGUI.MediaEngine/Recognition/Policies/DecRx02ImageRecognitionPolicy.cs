using GWGUI.MediaEngine.Containers.Dec.Rx02;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.Recognition.Dec;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Présélectionne un dump RX02 par sa capacité, le format demandé et son home block RT-11.</summary>
/// <param name="reader">Lecteur validant entièrement la capacité et l'ordre physique du dump depuis la mémoire partagée.</param>
internal sealed class DecRx02ImageRecognitionPolicy(DecRx02Reader reader) : ReaderBackedRecognitionPolicy(async (context, cancellationToken) => await reader.ReadAsync(await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false))
{
    /// <summary>Vérifie la capacité RX02 puis accepte une sélection explicite ou un home block RT-11 crédible.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture du contenu partagé.</param>
    /// <returns><see langword="true"/> pour un dump de capacité RX02 explicitement demandé ou contenant RT-11.</returns>
    public override async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.Length != DecRx02Geometry.Capacity) return false;
        if (context.RequestedFormatId?.Equals(DiskImageFormatIds.DecRx02, StringComparison.OrdinalIgnoreCase) == true) return true;
        return DecRx02ImageProbe.LooksLikeRt11(await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false));
    }
}
