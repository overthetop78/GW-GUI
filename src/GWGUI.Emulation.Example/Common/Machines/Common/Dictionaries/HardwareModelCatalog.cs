using GWGUI.Emulation.Example.Common.Machines.ExampleMachineA.Dictionaries;
using GWGUI.Emulation.Example.Common.Machines.ExampleMachineB.Dictionaries;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;

/// <summary>Catalogue unique des deux descriptions matérielles du module.</summary>
/// <remarks>Le regroupement conserve l’ordre A puis B ; l’index rejette les identités déclarées deux fois.</remarks>
public static class HardwareModelCatalog
{
    /// <summary>Descriptions stables des profils A et B, indépendantes du choix d’émulateur.</summary>
    public static IReadOnlyList<HardwareModelDefinition> All { get; } = Array.AsReadOnly(ExampleMachineAModelCatalog.All.Concat(ExampleMachineBModelCatalog.All).ToArray());
    /// <summary>Index privé par identité typée, construit une seule fois.</summary>
    private static readonly IReadOnlyDictionary<MachineModel, HardwareModelDefinition> ByModel = All.ToDictionary(item => item.Model);
    /// <summary>Recherche le profil matériel sans charger son cœur.</summary>
    /// <param name="model">Identité déclarée dans l’énumération du module.</param>
    /// <returns>Description matérielle stable du profil demandé.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Le modèle n’appartient pas au catalogue.</exception>
    public static HardwareModelDefinition Get(MachineModel model) => ByModel.TryGetValue(model, out var definition) ? definition : throw new ArgumentOutOfRangeException(nameof(model), model, null);
}
