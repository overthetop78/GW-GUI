namespace GWGUI.Emulation.Commodore.Common.Interfaces;

/// <summary>Description des réglages avec un modèle matériel fourni explicitement ou résolu depuis la configuration.</summary>
/// <remarks>La projection explicite conserve les données du modèle choisi par son appelant.
/// L’entrée commune résout le modèle de la configuration avant de réutiliser cette projection.</remarks>
internal interface IEmulatorSettingsAdapter : IEmulatorSettings
{
    /// <summary>Décrit les réglages à partir du modèle et de la configuration fournis.</summary>
    /// <param name="model">Description matérielle à présenter, conservée sous la responsabilité de son appelant.</param>
    /// <param name="configuration">Valeurs courantes des réglages de la machine.</param>
    /// <returns>Blocs de réglages du backend, avec les filtres de son implémentation existante.</returns>
    IReadOnlyList<EmulationSettingsBlock> DescribeSettings(Model model, MachineConfiguration configuration);
    /// <summary>Réutilise la projection explicite avec le modèle de la configuration.</summary>
    IReadOnlyList<EmulationSettingsBlock> IEmulatorSettings.DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context)
        => DescribeSettings(ModelCatalog.Get(configuration.Model), configuration);
}
