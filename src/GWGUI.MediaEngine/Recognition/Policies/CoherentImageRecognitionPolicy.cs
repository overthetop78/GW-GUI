using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Reconnaît une image brute Coherent par les champs internes de son superbloc, indépendamment de son extension.</summary>
/// <param name="reader">Lecteur chargé de valider et reconstruire l'image Coherent.</param>
internal sealed class CoherentImageRecognitionPolicy(CoherentImageReader reader) : IDiskImageRecognitionPolicy
{
    /// <summary>Vérifie la signature interne du superbloc Coherent dans le contenu partagé.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns><see langword="true"/> lorsque le superbloc Coherent est reconnu ; sinon <see langword="false"/>.</returns>
    public async ValueTask<bool> CanReadAsync(
        DiskImageRecognitionContext context,
        CancellationToken cancellationToken) =>
        CoherentImageReader.LooksLikeCoherent(
            await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false));

    /// <summary>Lit l'image brute dont le superbloc a été présélectionné.</summary>
    /// <param name="context">Contexte de l'image Coherent.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle Coherent reconstruite selon la géométrie zonée.</returns>
    /// <exception cref="InvalidDataException">Le lecteur rejette le superbloc, la taille ou la géométrie.</exception>
    public Task<SectorImage> ReadAsync(
        DiskImageRecognitionContext context,
        CancellationToken cancellationToken) => reader.ReadAsync(context.Path, cancellationToken);
}
