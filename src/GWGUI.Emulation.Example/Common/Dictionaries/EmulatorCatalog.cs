using GWGUI.Emulation;
using GWGUI.Emulation.Example.Common.Interfaces;
using GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries;

namespace GWGUI.Emulation.Example.Common.Dictionaries;

/// <summary>Catalogue des adaptateurs du module : découverte, descriptions, création et sélection par modèle.</summary>
/// <remarks>La découverte passe par le contrat public IEmulationAdapters fourni par le SDK et porte uniquement sur
/// l’assembly du module. Les descriptions et types découverts sont mémorisés ; les adaptateurs destinés aux opérations
/// sont recréés séparément. Le catalogue ne crée ni cœur, ni thread, ni processus d’émulation.</remarks>
internal static class EmulatorCatalog
{
    /// <summary>Inscription interne réunissant les métadonnées d’un adaptateur découvert.</summary>
    /// <param name="AdapterType">Type concret à instancier pour chaque demande d’adaptateur.</param>
    /// <param name="Emulator">Identité typée correspondant à la clé de l’adaptateur.</param>
    /// <param name="Definition">Description publique du cœur et de ses machines compatibles.</param>
    /// <param name="Defaults">Déclaration facultative de préférence pour les modèles, conservée pour consultation.</param>
    /// <remarks>Les constructeurs des adaptateurs et leurs déclarations de préférence doivent rester des opérations
    /// de description, sans acquérir de ressources de session nécessitant une destruction.</remarks>
    private sealed record Registration(Type AdapterType, Emulator Emulator, EmulationEmulatorDefinition Definition, IEmulatorMachineDefaults? Defaults);

    /// <summary>Inscriptions découvertes une seule fois, triées par identifiant public ordinal.</summary>
    private static readonly IReadOnlyList<Registration> Registrations = Discover();
    /// <summary>Accès aux inscriptions par identité typée ; les doublons sont rejetés à la construction de la table.</summary>
    private static readonly IReadOnlyDictionary<Emulator, Registration> ByEmulator = Registrations.ToDictionary(entry => entry.Emulator);

    /// <summary>Descriptions publiques mémorisées, dans l’ordre ordinal de leurs identifiants.</summary>
    /// <remarks>La collection exposée est en lecture seule et ne contient aucune instance de machine.</remarks>
    internal static IReadOnlyList<EmulationEmulatorDefinition> All { get; } = Array.AsReadOnly(Registrations.Select(entry => entry.Definition).ToArray());

    /// <summary>Découvre les adaptateurs et projette les informations nécessaires au catalogue.</summary>
    /// <returns>Inscriptions triées ; une clé d’émulateur invalide échoue lors de sa conversion typée.</returns>
    private static IReadOnlyList<Registration> Discover() => EmulationSdk.Adapters.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly)
        .Select(adapter => new Registration(adapter.GetType(), Enum.Parse<Emulator>(adapter.EmulatorKey), adapter.Definition, adapter as IEmulatorMachineDefaults))
        .OrderBy(entry => entry.Definition.Id, StringComparer.Ordinal).ToArray();

    /// <summary>Crée de nouvelles instances pour toutes les inscriptions connues.</summary>
    /// <returns>Adaptateurs indépendants dans l’ordre du catalogue ; leurs machines seront créées séparément.</returns>
    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => Registrations.Select(entry => CreateAdapter(entry.Emulator)).ToArray();

    /// <summary>Instancie l’adaptateur concret d’une identité d’émulateur.</summary>
    /// <param name="emulator">Identité typée à rechercher dans les inscriptions.</param>
    /// <returns>Nouvelle instance, avec la politique existante de constructeur sans paramètre éventuellement non public.</returns>
    /// <exception cref="ArgumentOutOfRangeException">L’identité ne possède aucune inscription.</exception>
    internal static IEmulatorAdapter CreateAdapter(Emulator emulator) => ByEmulator.TryGetValue(emulator, out var entry)
        ? (IEmulatorAdapter)Activator.CreateInstance(entry.AdapterType, nonPublic: true)! : throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null);

    /// <summary>Consulte la définition d’un émulateur sans instancier de nouvel adaptateur.</summary>
    /// <param name="emulator">Identité typée du cœur.</param>
    /// <returns>Description publique mémorisée.</returns>
    /// <exception cref="ArgumentOutOfRangeException">L’identité ne possède aucune inscription.</exception>
    internal static EmulationEmulatorDefinition Get(Emulator emulator) => ByEmulator.TryGetValue(emulator, out var entry)
        ? entry.Definition : throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null);

    /// <summary>Filtre les descriptions selon les modèles compatibles annoncés par les adaptateurs.</summary>
    /// <param name="machineId">Identifiant technique du modèle, indépendant de son libellé traduit.</param>
    /// <returns>Nouveau tableau dans l’ordre du catalogue, vide si aucun adaptateur ne prend en charge le modèle.</returns>
    internal static IReadOnlyList<EmulationEmulatorDefinition> GetAll(string machineId) => All.Where(definition => definition.MachineIds.Contains(machineId)).ToArray();

    /// <summary>Choisit le cœur par défaut d’un modèle à partir des adaptateurs compatibles.</summary>
    /// <param name="model">Modèle déclaré par l’énumération du module.</param>
    /// <returns>Premier choix explicitement recommandé, sinon premier choix compatible dans l’ordre de l’énumération,
    /// sinon Emulator.None.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La valeur du modèle n’est pas déclarée.</exception>
    /// <remarks>Les préférences explicites sont départagées par l’ordre ordinal des identifiants du catalogue.
    /// La sélection porte sur les capacités déclarées, pas sur la présence d’une installation téléchargée.</remarks>
    internal static Emulator DefaultEmulator(MachineModel model)
    {
        if (!Enum.IsDefined(model)) throw new ArgumentOutOfRangeException(nameof(model));
        var machineId = HardwareModelCatalog.Get(model).StableModelId;
        var compatible = Registrations.Where(entry => entry.Definition.MachineIds.Contains(machineId)).ToArray();
        return (compatible.FirstOrDefault(entry => entry.Defaults?.IsDefaultFor(machineId) == true)
            ?? compatible.OrderBy(entry => entry.Emulator).FirstOrDefault())?.Emulator ?? Emulator.None;
    }
}
