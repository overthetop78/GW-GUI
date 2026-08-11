using GWGUI.MediaEngine.Containers.Coherent;
using GWGUI.MediaEngine.FileSystems.Coherent;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Présélectionne une image brute COHERENT par son superbloc, indépendamment de son extension.</summary>
/// <param name="reader">Lecteur validant entièrement le dump et sa géométrie depuis la mémoire partagée.</param>
internal sealed class CoherentImageRecognitionPolicy(CoherentRawImageReader reader) : ReaderBackedRecognitionPolicy(async (context, cancellationToken) => await reader.ReadAsync(await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false))
{
    /// <summary>Recherche les marqueurs du superbloc avant de confier le candidat au lecteur complet.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture du contenu partagé.</param>
    /// <returns><see langword="true"/> lorsque le superbloc COHERENT est plausible ; sinon <see langword="false"/>.</returns>
    public override async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => CoherentSuperblockProbe.LooksLikeCoherent((await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false)).Span);
}
