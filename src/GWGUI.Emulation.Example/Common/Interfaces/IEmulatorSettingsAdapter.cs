namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Description des réglages et options qu’un backend expose dans la configuration de machine.</summary>
/// <remarks>Les blocs sont des données de présentation ; les valeurs restent appliquées par les propriétaires
/// de configuration et de cœur. Les clés de ressources permettent à l’hôte de présenter ces données dans sa langue.</remarks>
internal interface IEmulatorSettingsAdapter : IEmulatorSettings
{
    /// <summary>Décrit les options natives utilisables pour construire les réglages du backend.</summary>
    /// <param name="configuration">Configuration facultative pour résoudre les options qui dépendent du modèle ou des valeurs choisies.</param>
    /// <param name="context">Contexte facultatif de gestion lorsque la lecture des options requiert l’installation du cœur et ses services.
    /// Un backend qui en a besoin doit le demander à son appelant, sans conserver ni détruire les services empruntés.</param>
    /// <returns>Descriptions d’options, choix disponibles et métadonnées de présentation.</returns>
    IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null);

    /// <summary>Résout la quantité de RAM correspondant au modèle et à ses options.</summary>
    /// <param name="configuration">Configuration empruntée à décrire.</param>
    /// <returns>Capacité de mémoire principale en KiB.</returns>
    int GetMemoryKib(MachineConfiguration configuration);
}
