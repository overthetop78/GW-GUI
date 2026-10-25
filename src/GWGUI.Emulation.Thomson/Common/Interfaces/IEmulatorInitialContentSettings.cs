namespace GWGUI.Emulation.Thomson.Common.Interfaces;

/// <summary>Sélection du contenu initial et description des sources nécessaires au démarrage du cœur.</summary>
/// <remarks>Le contenu initial sélectionné séparément reste distinct des médias associés aux lecteurs.
/// Le contrat décrit leurs sources avant préparation de session ; il ne possède pas les fichiers et ne
/// charge aucun cœur. Une adaptation native conserve ses règles de sélection et ses dépendances.</remarks>
internal interface IEmulatorInitialContentSettings
{
    /// <summary>Indique si le cœur permet de sélectionner un contenu initial distinct des lecteurs configurés.</summary>
    /// <remarks>Cette capacité décrit la sélection séparée dans les réglages. Les médias montés peuvent
    /// être utilisés au démarrage indépendamment de cette capacité et restent décrits par leurs lecteurs.</remarks>
    bool SupportsInitialContent { get; }

    /// <summary>Résout les sources du contenu chargé au démarrage dans leur ordre de sélection.</summary>
    /// <param name="configuration">Configuration empruntée contenant les contenus, leurs emplacements et leur état d’insertion.</param>
    /// <returns>Chemins du contenu initial et de ses dépendances, ou des médias insérés lorsque le cœur ne définit aucune résolution propre.</returns>
    /// <remarks>La résolution Common filtre les médias éjectés et trie les lecteurs. Le cœur peut utiliser
    /// son résolveur existant pour un contenu distinct, une liste de lecture ou des fichiers associés.</remarks>
    IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration);
}
