using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Centralise la délégation identique des politiques dont le lecteur consomme directement un chemin.</summary>
/// <param name="read">Fonction de lecture et de validation du candidat présélectionné.</param>
internal abstract class ReaderBackedRecognitionPolicy(Func<string, CancellationToken, Task<SectorImage>> read) : IDiskImageRecognitionPolicy
{
    /// <summary>Détermine si la politique doit tenter de lire le contexte.</summary>
    /// <param name="context">Contexte partagé pendant la reconnaissance.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la présélection.</param>
    /// <returns><see langword="true"/> lorsque le lecteur doit être essayé ; sinon <see langword="false"/>.</returns>
    public abstract ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken);

    /// <summary>Transmet le chemin du candidat présélectionné à la fonction de lecture commune.</summary>
    /// <param name="context">Contexte contenant le chemin à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle validée par le lecteur délégué.</returns>
    /// <exception cref="InvalidDataException">Le lecteur rejette le contenu comme incompatible.</exception>
    /// <exception cref="NotSupportedException">Le lecteur rejette la variante ou le format demandé.</exception>
    public Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => read(context.Path, cancellationToken);
}
