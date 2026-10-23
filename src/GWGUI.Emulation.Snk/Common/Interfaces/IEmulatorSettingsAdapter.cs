namespace GWGUI.Emulation.Snk.Common.Interfaces;

internal interface IEmulatorSettingsAdapter : IEmulatorSettings
{
    /// <summary>Réutilise la description Common existante pour le contrat commun de réglages.</summary>
    IReadOnlyList<EmulationSettingsBlock> IEmulatorSettings.DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context) => SettingsDescriptionFunctions.Create(configuration);
    /// <summary>Fournit les descriptions d’options par le même contrat que les autres adaptateurs.</summary>
    /// <param name="configuration">Configuration facultative permettant de filtrer les options propres au modèle.</param>
    /// <param name="context">Services de gestion empruntés lorsque le catalogue exige un cœur installé.</param>
    /// <returns>Catalogue existant, conservé dans son ordre et avec ses règles de visibilité.</returns>
    IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null);
}
