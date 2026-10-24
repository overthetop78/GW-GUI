using CommonCoreHostFunctions = GWGUI.Emulation.Microsoft.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Microsoft.Common.Dictionaries;

public static class EmulatorCatalog
{
    internal static IReadOnlyList<EmulationEmulatorDefinition> All => CreateAdapters().Select(adapter => adapter.Definition).OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray();

    /// <summary>Découvre les fabriques du module par le même contrat Common que les autres modules.</summary>
    /// <returns>Adaptateurs indépendants ; liste vide tant qu'aucune fabrique concrète n'est fournie dans cet assembly.</returns>
    /// <remarks>La découverte construit les descriptions sans démarrer de cœur ni de processus.
    /// L'ajout d'une fabrique implémentant IEmulatorAdapter suffit pour qu'elle soit prise en compte.</remarks>
    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => CommonCoreHostFunctions.Adapters.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly);


    public static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) => All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();
}

