using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition;

/// <summary>Essaie dans leur ordre d'enregistrement les politiques capables de reconnaître une image de média.</summary>
public sealed class DiskImageRecognitionRegistry
{
    /// <summary>Politiques parcourues dans l'ordre fourni au constructeur.</summary>
    private readonly IReadOnlyList<IDiskImageRecognitionPolicy> policies;

    /// <summary>Crée un registre utilisant l'ordre explicite des politiques fournies.</summary>
    /// <param name="policies">Politiques de reconnaissance ordonnées.</param>
    public DiskImageRecognitionRegistry(IReadOnlyList<IDiskImageRecognitionPolicy> policies) =>
        this.policies = policies;

    /// <summary>Parcourt les politiques compatibles jusqu'à ce que l'une d'elles lise complètement le contenu.</summary>
    /// <param name="path">Chemin du fichier à reconnaître.</param>
    /// <param name="requestedFormatId">Identifiant de format demandé, ou <see langword="null"/> pour la détection automatique.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la reconnaissance.</param>
    /// <returns>Première image sectorielle validée par une politique.</returns>
    /// <exception cref="NotSupportedException">Aucune politique ne valide le contenu.</exception>
    /// <exception cref="OperationCanceledException">Le jeton est annulé avant ou pendant le parcours.</exception>
    /// <exception cref="IOException">Le fichier ne peut pas être consulté ou lu.</exception>
    /// <exception cref="UnauthorizedAccessException">L'accès au fichier est refusé.</exception>
    public async Task<SectorImage> ReadAsync(
        string path,
        string? requestedFormatId,
        CancellationToken cancellationToken)
    {
        var context = new DiskImageRecognitionContext(path, requestedFormatId);
        foreach (var policy in policies)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await policy.CanReadAsync(context, cancellationToken).ConfigureAwait(false)) continue;
            try
            {
                return await policy.ReadAsync(context, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
            {
                // Le contenu ne valide pas ce candidat faible ; la politique suivante reçoit le même contexte.
            }
        }
        throw DiskImageRecognitionExceptions.UnsupportedExtension(context.Extension);
    }
}
