namespace GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;

/// <summary>Projection publique des deux profils matériels proposés à l’application.</summary>
public static class MachineCatalog
{
    /// <summary>Identifiants et clés de présentation transmis par la façade de module.</summary>
    /// <remarks>Les images facultatives seront résolues par l’hôte ; cette déclaration n’acquiert aucune surface graphique.</remarks>
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = Array.AsReadOnly(HardwareModelCatalog.All.Select(item => new EmulationMachineDefinition(item.StableModelId, item.DisplayNameResourceKey)).ToArray());
}
