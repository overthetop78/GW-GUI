namespace GWGUI.Emulation.Example.Common.Dictionaries;

/// <summary>Découverte et sélection des adaptateurs d’émulateur déclarés dans l’assembly du module.</summary>
/// <remarks>Le catalogue manipule IEmulatorAdapter et expose leurs descriptions publiques à l’application.
/// Les appels de découverte produisent de nouvelles instances d’adaptateurs ; ce catalogue ne conserve pas
/// une instance de cœur, une session ou une liste de versions téléchargées entre ses consultations.</remarks>
internal static class EmulatorCatalog
{
    /// <summary>Descriptions de tous les adaptateurs, classées par identifiant avec comparaison ordinale.</summary>
    /// <remarks>Chaque lecture découvre les adaptateurs puis produit un nouveau tableau de descriptions.</remarks>
    internal static IReadOnlyList<EmulationEmulatorDefinition> All => CreateAdapters().Select(adapter => adapter.Definition).OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray();

    /// <summary>Construit les adaptateurs concrets du module et les classe par identifiant technique.</summary>
    /// <returns>Nouvelles instances d’adaptateurs ; les machines seront construites séparément par leur méthode Create.</returns>
    /// <remarks>La découverte porte sur l’assembly de ce catalogue. Les adaptateurs sont reconnus par leur contrat,
    /// et non par une liste de noms de classes enregistrée dans l’application.</remarks>
    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() =>
        EmulationAdapterFunctions.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly)
            .OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal)
            .ToArray();

    /// <summary>Sélectionne l’unique adaptateur dont la clé correspond au nom de l’émulateur demandé.</summary>
    /// <param name="emulator">Identité d’émulateur déclarée par le module.</param>
    /// <returns>Nouvelle instance de l’adaptateur sélectionné.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Aucun adaptateur ne correspond à cette identité.</exception>
    /// <exception cref="InvalidOperationException">Plusieurs adaptateurs déclarent la même clé.</exception>
    /// <remarks>La comparaison ordinale distingue les majuscules des minuscules et ne dépend pas de la langue.</remarks>
    internal static IEmulatorAdapter CreateAdapter(Emulator emulator) => CreateAdapters().SingleOrDefault(adapter => string.Equals(adapter.EmulatorKey, emulator.ToString(), StringComparison.Ordinal)) ?? throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null);

    /// <summary>Retourne la description publique de l’adaptateur sélectionné.</summary>
    /// <param name="emulator">Identité à résoudre avec les mêmes contrôles que CreateAdapter.</param>
    /// <returns>Description du backend et des machines qu’il prend en charge.</returns>
    internal static EmulationEmulatorDefinition Get(Emulator emulator) => CreateAdapter(emulator).Definition;

    /// <summary>Filtre les descriptions selon les identifiants de machines acceptés par chaque adaptateur.</summary>
    /// <param name="machineId">Identifiant technique du modèle, indépendant de son libellé traduit.</param>
    /// <returns>Tableau des descriptions compatibles, dans l’ordre du catalogue ; tableau vide en l’absence de correspondance.</returns>
    internal static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) => All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();
}
