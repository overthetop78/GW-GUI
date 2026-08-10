using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Containers;

/// <summary>Reconnaît un conteneur SCP par sa signature et le transmet au service d’exploration du flux.</summary>
/// <param name="exploration">Service reconstruisant une image sectorielle depuis le conteneur SCP.</param>
/// <param name="supportedFormatIds">Formats sectoriels pouvant être explicitement demandés.</param>
internal sealed class ScpContainerPolicy(
    ScpImageExplorationService exploration,
    IReadOnlySet<string> supportedFormatIds) : IDiskImageContainerPolicy
{
    /// <summary>Vérifie la signature SCP dans le contenu, indépendamment de l’extension du fichier.</summary>
    /// <param name="context">Contexte du fichier à examiner.</param>
    /// <param name="cancellationToken">Jeton d’annulation de la lecture.</param>
    /// <returns><see langword="true"/> lorsque la signature SCP est présente.</returns>
    public async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        return bytes.Length >= ScpFormatConstants.SignatureLength &&
               bytes.AsSpan(0, ScpFormatConstants.SignatureLength).SequenceEqual(ScpFormatConstants.FileSignature);
    }

    /// <summary>Valide le format demandé puis lance la reconstruction de l’image SCP.</summary>
    /// <param name="context">Contexte du fichier à lire.</param>
    /// <param name="cancellationToken">Jeton d’annulation de l’opération.</param>
    /// <returns>Image sectorielle reconstruite depuis le flux SCP.</returns>
    /// <exception cref="NotSupportedException">Le format explicitement demandé n’est pas pris en charge.</exception>
    public Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !supportedFormatIds.Contains(context.RequestedFormatId))
            throw DiskImageRecognitionExceptions.UnsupportedRequestedFormat(context.RequestedFormatId, nameof(ScpContainerPolicy));
        return exploration.ReadAsync(context.Path, context.RequestedFormatId, cancellationToken);
    }
}
