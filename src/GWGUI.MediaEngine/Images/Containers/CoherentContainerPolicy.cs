using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Containers;

/// <summary>Reconnaît les images brutes dont le superbloc correspond à un système Coherent.</summary>
/// <param name="reader">Lecteur des images sectorielles Coherent.</param>
internal sealed class CoherentContainerPolicy(CoherentImageReader reader) : IDiskImageContainerPolicy
{
    /// <summary>Vérifie l’indice BIN puis la signature interne du superbloc Coherent.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton d’annulation de la lecture.</param>
    /// <returns><see langword="true"/> lorsque le superbloc est reconnu.</returns>
    public async ValueTask<bool> CanReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        context.Extension.Equals(DiskImageFileExtensions.Bin, StringComparison.OrdinalIgnoreCase) &&
        CoherentImageReader.LooksLikeCoherent(await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false));

    /// <summary>Lit l’image Coherent validée par la politique.</summary>
    /// <param name="context">Contexte du fichier à lire.</param>
    /// <param name="cancellationToken">Jeton d’annulation de l’opération.</param>
    /// <returns>Image sectorielle Coherent.</returns>
    public Task<SectorImage> ReadAsync(DiskImageContainerContext context, CancellationToken cancellationToken) =>
        reader.ReadAsync(context.Path, cancellationToken);
}
