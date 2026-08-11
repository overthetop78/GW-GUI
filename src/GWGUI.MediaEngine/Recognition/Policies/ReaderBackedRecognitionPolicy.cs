using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>Centralise la validation des politiques déléguant leur lecture à un Reader.</summary>
internal abstract class ReaderBackedRecognitionPolicy : IDiskImageRecognitionPolicy
{
    private readonly Func<DiskImageRecognitionContext, CancellationToken, Task<SectorImage>> read;

    /// <summary>Crée une délégation vers un Reader qui ouvre lui-même le chemin du candidat.</summary>
    /// <param name="read">Point d'entrée recevant le chemin et le jeton d'annulation.</param>
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

    /// <summary>Transmet le candidat présélectionné au point d'entrée configuré.</summary>
    /// <param name="context">Contexte contenant le chemin et la mémoire partagée.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle entièrement validée par le Reader délégué.</returns>
    /// <exception cref="InvalidDataException">Le Reader rejette le contenu comme incompatible.</exception>
    /// <exception cref="NotSupportedException">Le Reader rejette la variante ou le format demandé.</exception>
    public Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => read(context, cancellationToken);
}
