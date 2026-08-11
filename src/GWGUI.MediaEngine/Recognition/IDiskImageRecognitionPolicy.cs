using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition;

/// <summary>Définit la présélection et la validation complète d'un candidat à la reconnaissance.</summary>
public interface IDiskImageRecognitionPolicy
{
    /// <summary>Présélectionne un candidat sans garantir que son contenu est valide pour le lecteur.</summary>
    /// <param name="context">Informations et contenu partagé en lecture seule pendant la reconnaissance.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler l'examen du candidat.</param>
    /// <returns><see langword="true"/> lorsque la politique souhaite tenter une lecture complète ; sinon <see langword="false"/>.</returns>
    /// <exception cref="OperationCanceledException">Le jeton est annulé pendant l'examen.</exception>
    /// <exception cref="IOException">Le contenu partagé ne peut pas être lu.</exception>
    /// <exception cref="UnauthorizedAccessException">L'accès au fichier est refusé.</exception>
    ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken);

    /// <summary>Valide entièrement le candidat présélectionné et produit son image sectorielle.</summary>
    /// <param name="context">Informations et contenu partagé en lecture seule pendant la reconnaissance.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la validation et la lecture.</param>
    /// <returns>Image sectorielle validée et reconstruite par la politique.</returns>
    /// <exception cref="InvalidDataException">Le contenu présélectionné est incompatible avec le lecteur ; le registre peut essayer la politique suivante.</exception>
    /// <exception cref="NotSupportedException">La variante ou le format demandé n'est pas pris en charge ; le registre peut essayer la politique suivante.</exception>
    /// <exception cref="OperationCanceledException">Le jeton est annulé pendant la lecture.</exception>
    /// <exception cref="IOException">Le contenu partagé ne peut pas être lu.</exception>
    /// <exception cref="UnauthorizedAccessException">L'accès au fichier est refusé.</exception>
    Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken);
}
