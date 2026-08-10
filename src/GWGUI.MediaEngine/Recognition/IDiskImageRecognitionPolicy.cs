using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition;

/// <summary>Définit une étape de présélection et de lecture utilisée par le registre de reconnaissance.</summary>
public interface IDiskImageRecognitionPolicy
{
    /// <summary>Indique si la politique peut tenter de lire le fichier décrit par le contexte.</summary>
    /// <param name="context">Informations et contenu partagés pendant la reconnaissance.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la présélection.</param>
    /// <returns><see langword="true"/> lorsque la politique accepte d'essayer son lecteur ; sinon <see langword="false"/>.</returns>
    ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken);

    /// <summary>Lit le candidat présélectionné et produit son image sectorielle.</summary>
    /// <param name="context">Informations et contenu partagés pendant la reconnaissance.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle reconnue et reconstruite par la politique.</returns>
    /// <exception cref="InvalidDataException">Le contenu présélectionné est incompatible avec le lecteur.</exception>
    /// <exception cref="NotSupportedException">Une variante ou un format demandé n'est pas pris en charge.</exception>
    Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken);
}
