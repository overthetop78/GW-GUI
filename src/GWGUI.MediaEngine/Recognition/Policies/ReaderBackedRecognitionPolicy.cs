using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Centralise la délégation de lecture commune aux politiques adossées à un Reader.</summary>
internal abstract class ReaderBackedRecognitionPolicy : IDiskImageRecognitionPolicy
{
    /// <summary>Fonction configurée par la politique concrète pour choisir les données du contexte à transmettre au Reader.</summary>
    private readonly Func<DiskImageRecognitionContext, CancellationToken, Task<SectorImage>> read;

    /// <summary>Crée une délégation vers le Reader responsable de la validation complète du candidat.</summary>
    /// <param name="read">Fonction recevant le contexte complet et le jeton d'annulation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="read"/> est <see langword="null"/>.</exception>
    protected ReaderBackedRecognitionPolicy(Func<DiskImageRecognitionContext, CancellationToken, Task<SectorImage>> read)
    {
        ArgumentNullException.ThrowIfNull(read);
        this.read = read;
    }

    /// <summary>Détermine si la politique doit tenter de valider complètement le contexte.</summary>
    /// <param name="context">Contexte partagé pendant la reconnaissance.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la présélection.</param>
    /// <returns><see langword="true"/> lorsque le Reader doit être essayé ; sinon <see langword="false"/>.</returns>
    public abstract ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken);

    /// <summary>Transmet sans transformation le contexte présélectionné à la fonction configurée par la politique concrète.</summary>
    /// <param name="context">Contexte complet de reconnaissance à transmettre.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle entièrement validée par le Reader.</returns>
    /// <exception cref="InvalidDataException">La fonction de lecture rejette le contenu comme incompatible.</exception>
    /// <exception cref="NotSupportedException">La fonction de lecture rejette la variante ou le format demandé.</exception>
    public Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => read(context, cancellationToken);
}
