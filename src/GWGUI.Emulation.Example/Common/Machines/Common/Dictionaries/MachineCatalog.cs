using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;

/// <summary>Descriptions de machines présentées à l’application, incluant leur ressource d’image.</summary>
/// <remarks>ModelCatalog fournit l’identité et le nom traduit. Cette projection ajoute le nom logique
/// de l’image embarquée, sans charger ni posséder de ressource graphique.</remarks>
public static class MachineCatalog
{
    /// <summary>Descriptions construites une fois dans l’ordre de ModelCatalog.</summary>
    /// <remarks>Les modèles partagent l’image déclarée du groupe, sauf ExampleMachineC dont le nom de ressource
    /// est construit depuis son identifiant. La résolution et l’affichage des images appartiennent à l’hôte.</remarks>
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } = ModelCatalog.All
        .Select(model => new EmulationMachineDefinition(model.Id, model.DisplayResourceKey,
            ModelCatalog.Parse(model.Id) != MachineModel.ExampleMachineC ? EmulationModuleConstants.ExampleMachineAImageResourceName : $"{EmulationModuleConstants.AssetResourcePrefix}.Machines.{model.Id}.png")).ToArray();
}
