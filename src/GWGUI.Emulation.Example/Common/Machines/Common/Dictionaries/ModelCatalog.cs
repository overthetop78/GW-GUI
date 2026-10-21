using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;

/// <summary>Projection des modèles matériels vers les identifiants et clés de présentation du contrat public.</summary>
/// <remarks>L’identifiant utilisé ici est le nom de MachineModel, également exposé par la configuration.
/// La traduction du libellé ne modifie donc pas l’identité du modèle.</remarks>
public static class ModelCatalog
{
    /// <summary>Descriptions publiques des modèles, dans l’ordre du catalogue matériel.</summary>
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } =
        HardwareModelCatalog.All.Select(definition => new EmulationMachineDefinition(definition.Model.ToString(), definition.DisplayNameResourceKey)).ToArray();

    /// <summary>Convertit un identifiant textuel vers une valeur déclarée de MachineModel.</summary>
    /// <param name="id">Nom exact du modèle, ou représentation numérique d’une valeur déclarée dans l’énumération.</param>
    /// <returns>Valeur typée reconnue ; la conversion des noms distingue majuscules et minuscules.</returns>
    /// <exception cref="ArgumentOutOfRangeException">L’identifiant ne désigne aucune valeur déclarée.</exception>
    public static MachineModel Parse(string id) => GWGUI.Emulation.Functions.EmulationMachineModelFunctions.Parse<MachineModel>(id);
}
