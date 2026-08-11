using GWGUI.MediaEngine.Images;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Reconnaît une image brute Coherent par les champs internes de son superbloc, indépendamment de son extension.</summary>
/// <param name="reader">Lecteur chargé de valider et reconstruire l'image Coherent.</param>
internal sealed class CoherentImageRecognitionPolicy(CoherentImageReader reader) : ReaderBackedRecognitionPolicy(reader.ReadAsync)
{
    /// <summary>Vérifie la signature interne du superbloc Coherent dans le contenu partagé.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns><see langword="true"/> lorsque le superbloc Coherent est reconnu ; sinon <see langword="false"/>.</returns>
    public override async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) =>
        CoherentImageReader.LooksLikeCoherent((await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false)).Span);
}
