using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Containers;

/// <summary>Présélectionne une fonction de lecture à partir d’un ensemble d’indices d’extension.</summary>
/// <param name="read">Fonction chargée de lire et valider le contenu présélectionné.</param>
/// <param name="extensions">Extensions normalisées acceptées comme indices.</param>
internal sealed class DelegatingContainerPolicy(
    Func<string, CancellationToken, Task<SectorImage>> read,
    params string[] extensions) : IDiskImageContainerPolicy
{
    private readonly HashSet<string> supportedExtensions = new(extensions, StringComparer.OrdinalIgnoreCase);

    /// <summary>Indique si l’extension du contexte appartient aux indices configurés.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton d’annulation de l’opération.</param>
    /// <returns><see langword="true"/> lorsque l’extension est configurée.</returns>
    public ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(supportedExtensions.Contains(context.Extension));

    /// <summary>Transmet le fichier présélectionné à la fonction de lecture configurée.</summary>
    /// <param name="context">Contexte du fichier à lire.</param>
    /// <param name="cancellationToken">Jeton d’annulation de l’opération.</param>
    /// <returns>Image sectorielle validée par la fonction.</returns>
    public Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        read(context.Path, cancellationToken);
}
