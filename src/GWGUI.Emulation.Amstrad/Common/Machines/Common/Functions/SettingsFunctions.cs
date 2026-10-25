namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

/// <summary>Sélection de l’adaptateur et acheminement des données nécessaires à la description de ses réglages.</summary>
internal static class SettingsDescriptionFunctions
{
    /// <summary>Demande la description commune avec les services nécessaires à la lecture des options.</summary>
    /// <param name="configuration">Configuration empruntée à présenter.</param>
    /// <param name="context">Services de gestion facultatifs, empruntés pendant la description et conservés par leur propriétaire.</param>
    /// <returns>Blocs de l’adaptateur après la lecture habituelle des options.</returns>
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration, EmulatorManagementContext? context = null)
        => new Engine().Adapter(configuration).GetMachineSettingsBlocks(configuration, context: context);
}
