namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

/// <summary>Sélection de l’adaptateur et acheminement des données nécessaires à la description de ses réglages.</summary>
internal static class SettingsDescriptionFunctions
{
    /// <summary>Projette des descriptions natives déjà chargées ; une liste absente conserve le repli vide existant.</summary>
    /// <param name="configuration">Configuration empruntée à présenter.</param>
    /// <param name="coreOptions">Options natives disponibles, ou null pour la projection sans options.</param>
    /// <returns>Blocs de l’adaptateur sélectionné.</returns>
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration, IReadOnlyList<CoreOption>? coreOptions = null)
        => new Engine().Adapter(configuration).DescribeSettings(configuration, coreOptions ?? []);
    /// <summary>Demande la description commune avec les services nécessaires à la lecture des options.</summary>
    /// <param name="configuration">Configuration empruntée à présenter.</param>
    /// <param name="context">Contexte de gestion emprunté ; l’adaptateur le conserve sous la responsabilité de son appelant.</param>
    /// <returns>Blocs de l’adaptateur après la lecture habituelle des options.</returns>
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration, EmulatorManagementContext context)
        => new Engine().Adapter(configuration).DescribeSettings(configuration, context: context);
}
