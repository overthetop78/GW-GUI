namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Description des réglages et options qu’un backend expose dans la configuration de machine.</summary>
/// <remarks>Les blocs sont des données de présentation ; les valeurs restent appliquées par les propriétaires
/// de configuration et de cœur. Les clés de ressources permettent à l’hôte de présenter ces données dans sa langue.</remarks>
internal interface IEmulatorSettings
{
    /// <summary>Décrit les blocs de réglages correspondant au modèle et à la configuration courants.</summary>
    /// <param name="configuration">Configuration empruntée à présenter, sans modification.</param>
    /// <returns>Blocs et éditeurs de réglages du backend, avec leurs valeurs courantes.</returns>
    IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration);
    /// <summary>Décrit les options natives utilisables pour construire les réglages du backend.</summary>
    /// <param name="configuration">Configuration facultative pour résoudre les options qui dépendent du modèle ou des valeurs choisies.</param>
    /// <returns>Descriptions d’options, choix disponibles et métadonnées de présentation.</returns>
    IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null);
}
