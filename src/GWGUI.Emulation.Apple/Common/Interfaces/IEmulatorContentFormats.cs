namespace GWGUI.Emulation.Apple.Common.Interfaces;

/// <summary>Description des fichiers associés au contenu chargé par le cœur.</summary>
/// <remarks>Common utilise ces chemins pour identifier un contenu complet. Les fichiers de l’utilisateur
/// restent empruntés ; cette description ne prépare ni ne monte elle-même un média.</remarks>
internal interface IEmulatorContentFormats
{
    /// <summary>Décrit les dépendances du fichier principal suivant les règles de son format.</summary>
    /// <param name="path">Chemin du contenu principal dont les fichiers associés sont demandés.</param>
    /// <returns>Chemins des fichiers associés, dans l’ordre déterminé par le backend ; une liste vide indique un fichier autonome.</returns>
    IReadOnlyList<string> GetMediaDependencies(string path);
}
