namespace GWGUI.MediaEngine.SectorImages.Reading;

/// <summary>Définit la lecture commune d'un conteneur produisant une image sectorielle.</summary>
public interface ISectorImageReader
{
    /// <summary>Indique, à partir du chemin, si le lecteur peut être un candidat pour ce fichier.</summary>
    /// <param name="path">Chemin du fichier à examiner.</param>
    /// <returns><see langword="true"/> lorsque le chemin constitue un indice compatible ; cette réponse ne valide pas le contenu.</returns>
    bool CanRead(string path);

    /// <summary>Lit le fichier et construit son image sectorielle.</summary>
    /// <param name="path">Chemin du fichier à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>L'image sectorielle reconstruite.</returns>
    Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default);
}
