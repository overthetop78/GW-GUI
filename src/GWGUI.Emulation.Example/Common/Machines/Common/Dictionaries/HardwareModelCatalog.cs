namespace GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;

/// <summary>Catalogue matériel commun réunissant les définitions des modèles de machines.</summary>
/// <remarks>Les catalogues spécifiques fournissent les définitions complètes ; ce propriétaire les expose
/// dans leur ordre de déclaration et construit l’index permettant de retrouver un modèle par son identité typée.</remarks>
public static class HardwareModelCatalog
{
    // Liste de définitions fournie par le catalogue spécifique, conservée sans copie supplémentaire.
    private static readonly IReadOnlyList<HardwareModelDefinition> Definitions = ExampleMachineAModelCatalog.All;
    // L’indexation interdit les identités de modèles en double et permet une recherche directe.
    private static readonly IReadOnlyDictionary<MachineModel, HardwareModelDefinition> ByModel = HardwareModelFunctions.Index(Definitions);

    /// <summary>Définitions matérielles dans l’ordre du catalogue spécifique.</summary>
    public static IReadOnlyList<HardwareModelDefinition> All => Definitions;

    /// <summary>Retrouve la définition matérielle d’un modèle du module.</summary>
    /// <param name="model">Identité typée à rechercher dans l’index.</param>
    /// <returns>Définition existante du catalogue, dont les collections doivent rester stables.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Aucune définition ne correspond au modèle demandé.</exception>
    public static HardwareModelDefinition Get(MachineModel model) => ByModel.TryGetValue(model, out var definition) ? definition : throw new ArgumentOutOfRangeException(nameof(model), model, null);
}
