namespace GWGUI.Emulation.Acorn.Common.Interfaces;

/// <summary>Formats de contenu, contraintes de chargement et fichiers associés décrits par le backend.</summary>
/// <remarks>Ces opérations décrivent le contenu utilisable ; elles ne le montent ni ne le convertissent.
/// Les fichiers éventuellement consultés pour identifier les dépendances restent sous la propriété de l’appelant.</remarks>
internal interface IEmulatorContentFormats
{
    /// <summary>Extensions de contenus annoncées par l’adaptateur sans démarrer une machine.</summary>
    IReadOnlyList<string> SupportedContentExtensions { get; }

    /// <summary>Résout les extensions effectivement utilisables à partir des déclarations natives.</summary>
    /// <param name="declaredExtensions">Ensemble emprunté d’extensions annoncé par le cœur.</param>
    /// <param name="requiresFilePath">True si le cœur exige un chemin plutôt qu’un contenu en mémoire.</param>
    /// <returns>Extensions conservées après application des contraintes de l’adaptateur.</returns>
    IReadOnlySet<string> ResolveContentExtensions(IReadOnlySet<string> declaredExtensions, bool requiresFilePath);

    /// <summary>Décrit les fichiers nécessaires au chargement d’un média, notamment les pistes référencées par un conteneur.</summary>
    /// <param name="path">Chemin du média principal.</param>
    /// <returns>Chemins des dépendances du média ; une liste vide indique qu’aucun fichier associé n’est nécessaire.</returns>
    IReadOnlyList<string> GetMediaDependencies(string path);
}
