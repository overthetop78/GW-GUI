namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Sélection du contenu chargé à l’initialisation, distinct des médias insérés ensuite.</summary>
internal interface IEmulatorInitialContentSettings
{
    /// <summary>Indique si l’adaptateur permet de choisir un contenu initial dans les réglages.</summary>
    bool SupportsInitialContent { get; }
    /// <summary>Résout les chemins de contenu initial dans leur ordre de chargement.</summary>
    /// <param name="configuration">Configuration empruntée contenant les choix de démarrage.</param>
    /// <returns>Chemins préparés pour l’initialisation du cœur, sans transfert de propriété des fichiers.</returns>
    IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration);
}
