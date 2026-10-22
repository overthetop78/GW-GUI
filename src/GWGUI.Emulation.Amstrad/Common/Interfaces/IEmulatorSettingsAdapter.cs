namespace GWGUI.Emulation.Amstrad.Common.Interfaces;

/// <summary>Description des réglages à partir d’options natives déjà chargées ou du contexte de gestion.</summary>
/// <remarks>Le contrat commun compose les opérations existantes sans charger deux fois les options.
/// L’appelant conserve les services de gestion et les listes de descriptions qu’il transmet.</remarks>
internal interface IEmulatorSettingsAdapter : IEmulatorSettings
{
    /// <summary>Projette les options déjà disponibles dans les blocs de réglages du backend.</summary>
    /// <param name="configuration">Configuration empruntée dont les valeurs doivent être présentées.</param>
    /// <param name="coreOptions">Descriptions natives préchargées par l’appelant.</param>
    /// <returns>Blocs du backend construits par son implémentation existante.</returns>
    IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, IReadOnlyList<CoreOption> coreOptions);
    /// <summary>Lit les descriptions d’options avec les services de gestion fournis.</summary>
    /// <param name="configuration">Configuration facultative, suivant le contrat commun des adaptateurs.</param>
    /// <param name="context">Contexte emprunté de l’installation du cœur ; null conserve le catalogue vide du parcours sans contexte.</param>
    /// <returns>Descriptions natives utilisables pour présenter ses réglages.</returns>
    IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null);
    /// <summary>Compose la lecture des options et leur projection pour le contrat commun.</summary>
    /// <remarks>Sans contexte, la projection conserve la liste vide employée par le parcours existant.</remarks>
    IReadOnlyList<EmulationSettingsBlock> IEmulatorSettings.DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context)
    {
        IReadOnlyList<CoreOption> coreOptions = context is null ? [] : GetSettingsOptions(configuration, context);
        return DescribeSettings(configuration, coreOptions);
    }
}
