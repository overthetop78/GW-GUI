namespace GWGUI.Emulation.Snk.Common.Interfaces;

/// <summary>Résolution du contenu chargé lors de l’initialisation du cœur.</summary>
/// <remarks>La description des lecteurs relève de IEmulatorMediaCapabilities. Ce contrat conserve
/// les règles de sélection et l’ordre des contenus préparés pour le démarrage, sans posséder leurs fichiers.</remarks>
internal interface IEmulatorInitialContentSettings
{
    /// <summary>Résout les chemins de contenu initial de la configuration.</summary>
    /// <param name="configuration">Configuration empruntée contenant le modèle, le cœur et le contenu sélectionné.</param>
    /// <returns>Chemins chargés à l’initialisation dans leur ordre existant ; liste vide en l’absence de contenu.</returns>
    IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration);
}
