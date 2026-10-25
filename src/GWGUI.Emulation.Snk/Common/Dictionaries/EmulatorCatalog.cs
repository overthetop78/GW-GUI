using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Snk.Common.Dictionaries;

/// <summary>Catalogue des adaptateurs découverts dans l’assembly du module, avec leurs définitions publiques.</summary>
/// <remarks>Les descriptions sont mémorisées sans créer de cœur ni de session. Les adaptateurs destinés aux
/// opérations sont recréés séparément ; leurs constructeurs restent des opérations de description.</remarks>
internal static class EmulatorCatalog
{
    /// <summary>Inscription interne associant l’identité typée, la fabrique et sa définition SDK.</summary>
    /// <param name="AdapterType">Type concret à instancier à chaque demande d’adaptateur.</param>
    /// <param name="Emulator">Identité typée correspondant à la clé annoncée par l’adaptateur.</param>
    /// <param name="Definition">Description publique conservée avec ses noms et identités d’origine.</param>
    private sealed record Registration(Type AdapterType, Emulator Emulator, EmulationEmulatorDefinition Definition);

    /// <summary>Inscriptions découvertes une seule fois et triées par identifiant public ordinal.</summary>
    private static readonly IReadOnlyList<Registration> Registrations = Discover();
    /// <summary>Accès aux inscriptions par identité typée ; les doublons de clés sont rejetés.</summary>
    private static readonly IReadOnlyDictionary<Emulator, Registration> ByEmulator = Registrations.ToDictionary(entry => entry.Emulator);
    /// <summary>Définitions SDK dans l’ordre du catalogue, sans projection vers un second contrat de métadonnées.</summary>
    internal static IReadOnlyList<EmulationEmulatorDefinition> All { get; } = Array.AsReadOnly(Registrations.Select(entry => entry.Definition).ToArray());

    /// <summary>Découvre les adaptateurs par l’interface Common et valide leurs identités.</summary>
    /// <returns>Inscriptions dont les modèles sont interprétables et les identifiants publics uniques.</returns>
    /// <remarks>La validation ne conserve pas de seconde table par identifiant. Les métadonnées restent celles
    /// de la définition SDK ; les identités de modèles sont converties lorsque les règles du module les consultent.</remarks>
    private static IReadOnlyList<Registration> Discover()
    {
        var registrations = CommonCoreHostFunctions.Adapters.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly)
            .Select(adapter => new Registration(adapter.GetType(), Enum.Parse<Emulator>(adapter.EmulatorKey), adapter.Definition))
            .OrderBy(entry => entry.Definition.Id, StringComparer.Ordinal).ToArray();
        var identifiers = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in registrations)
        {
            if (!identifiers.Add(entry.Definition.Id)) throw new ArgumentException(ErrorMessages.DuplicateCore);
            foreach (var model in entry.Definition.MachineIds) _ = Enum.Parse<MachineModel>(model);
        }
        return registrations;
    }

    /// <summary>Crée de nouvelles instances d’adaptateurs dans l’ordre du catalogue.</summary>
    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => Registrations.Select(entry => CreateAdapter(entry.Emulator)).ToArray();

    /// <summary>Instancie la fabrique correspondant à l’identité typée, sans charger de cœur.</summary>
    /// <param name="emulator">Identité d’émulateur à résoudre.</param>
    /// <returns>Nouvel adaptateur indépendant ; aucune session n’est démarrée.</returns>
    internal static IEmulatorAdapter CreateAdapter(Emulator emulator) => ByEmulator.TryGetValue(emulator, out var entry)
        ? (IEmulatorAdapter)Activator.CreateInstance(entry.AdapterType, nonPublic: true)! : throw new ArgumentOutOfRangeException(nameof(emulator));

    /// <summary>Consulte la définition publique mémorisée d’un émulateur.</summary>
    /// <param name="emulator">Identité typée du cœur.</param>
    /// <returns>La même définition SDK que celle exposée dans All.</returns>
    internal static EmulationEmulatorDefinition Get(Emulator emulator) => ByEmulator.TryGetValue(emulator, out var entry)
        ? entry.Definition : throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null);
}
