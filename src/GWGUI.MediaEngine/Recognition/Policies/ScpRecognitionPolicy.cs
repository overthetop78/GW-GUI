using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition.Policies;

/// <summary>ReconnaÃ®t le conteneur SCP, vÃ©rifie la demande sectorielle puis confie sa reconstruction au service SCP.</summary>
internal sealed class ScpRecognitionPolicy : IDiskImageRecognitionPolicy
{
    /// <summary>Service reconstruisant une image sectorielle depuis le flux SCP.</summary>
    private readonly ScpImageExplorationService exploration;
    /// <summary>Copie insensible Ã  la casse des formats sectoriels explicitement acceptÃ©s.</summary>
    private readonly HashSet<string> supportedFormatIds;

    /// <summary>CrÃ©e la politique avec le service SCP et une copie des identifiants pris en charge.</summary>
    /// <param name="exploration">Service de reconstruction sectorielle SCP.</param>
    /// <param name="supportedFormatIds">Identifiants sectoriels pouvant Ãªtre demandÃ©s explicitement.</param>
    public ScpRecognitionPolicy(ScpImageExplorationService exploration, IReadOnlySet<string> supportedFormatIds)
    {
        this.exploration = exploration;
        this.supportedFormatIds = new(supportedFormatIds, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>ReconnaÃ®t la signature SCP complÃ¨te dans le contenu, indÃ©pendamment de l'extension.</summary>
    /// <param name="context">Contexte du fichier Ã  examiner.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture du contexte.</param>
    /// <returns><see langword="true"/> lorsque la signature SCP est prÃ©sente.</returns>
    public async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => ScpSignature.IsPresent((await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false)).Span);

    /// <summary>VÃ©rifie le format demandÃ© puis lance la reconstruction sectorielle SCP explicite ou automatique.</summary>
    /// <param name="context">Contexte contenant le chemin et la demande sectorielle Ã©ventuelle.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler l'exploration.</param>
    /// <returns>Image sectorielle reconstruite depuis le flux SCP.</returns>
    /// <exception cref="NotSupportedException">Le format explicitement demandÃ© n'est pas pris en charge.</exception>
    public Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !supportedFormatIds.Contains(context.RequestedFormatId)) throw DiskImageRecognitionExceptions.PolicyDoesNotSupportRequestedFormat(context.RequestedFormatId, nameof(ScpRecognitionPolicy));
        return exploration.ReadAsync(context.Path, context.RequestedFormatId, cancellationToken);
    }
}
