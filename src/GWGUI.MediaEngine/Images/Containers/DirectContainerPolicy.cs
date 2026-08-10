using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Containers;

/// <summary>Présélectionne un lecteur sectoriel à partir d’un ensemble d’indices d’extension.</summary>
/// <param name="reader">Lecteur sectoriel chargé de valider le contenu présélectionné.</param>
/// <param name="extensions">Extensions normalisées acceptées comme indices.</param>
internal sealed class DirectContainerPolicy(ISectorImageReader reader, params string[] extensions)
    : IDiskImageContainerPolicy
{
    private readonly HashSet<string> supportedExtensions = new(extensions, StringComparer.OrdinalIgnoreCase);

    /// <summary>Indique si l’extension du contexte appartient aux indices configurés.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton d’annulation de l’opération.</param>
    /// <returns><see langword="true"/> lorsque l’extension est configurée.</returns>
    public ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(supportedExtensions.Contains(context.Extension));

    /// <summary>Transmet le fichier présélectionné au lecteur sectoriel configuré.</summary>
    /// <param name="context">Contexte du fichier à lire.</param>
    /// <param name="cancellationToken">Jeton d’annulation de l’opération.</param>
    /// <returns>Image sectorielle validée par le lecteur.</returns>
    public Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        reader.ReadAsync(context.Path, cancellationToken);
}
