using System.Text.Json.Serialization;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;



/// <summary>Emplacements configurables des données utilisées ou produites par une machine.</summary>
/// <param name="Shared">Dossier partagé avec la machine, ou null pour laisser la résolution au module.</param>
/// <param name="Floppies">Dossier de travail des images de disquettes, ou null pour le choix du module.</param>
/// <param name="States">Dossier des sauvegardes d’état, ou null pour le choix du module.</param>
/// <param name="Captures">Dossier des captures, ou null pour le choix du module.</param>
/// <remarks>Ce contrat décrit des chemins ; sa construction ne crée ni ne supprime les dossiers.
/// Le service qui ouvre ou crée une ressource reste responsable de sa durée de vie.</remarks>
public sealed record FolderConfiguration(string? Shared = null, string? Floppies = null, string? States = null, string? Captures = null);

/// <summary>Configuration persistante d’une machine, échangée avec l’hôte par IEmulationConfiguration.</summary>
/// <remarks>La construction résout le backend et les éventuelles variantes de modèle, copie les listes de
/// firmwares et de médias, puis complète les options absentes par les valeurs initiales de l’adaptateur.
/// Les options fournies restent prioritaires. La validation Common termine la construction.
/// Les entrées et leurs collections restent des données empruntées à conserver stables ; les propriétés init
/// permettent des copies avec « with », dont les valeurs doivent être validées avant enregistrement ou exécution.</remarks>
public sealed record MachineConfiguration : IEmulationConfiguration
{
    /// <summary>Reconstruit le document JSON par le même parcours de préparation et de validation qu’une nouvelle configuration.</summary>
    /// <param name="schemaVersion">Version du schéma enregistrée dans le document.</param>
    /// <param name="id">Identité persistante ; un GUID vide entraîne la création d’une nouvelle identité.</param>
    /// <param name="model">Modèle déclaré dans le document.</param>
    /// <param name="core">Backend déclaré ; la construction résout sa compatibilité avec le modèle.</param>
    /// <param name="firmwares">Firmwares configurés ; la liste est copiée.</param>
    /// <param name="media">Montages configurés ; la liste est copiée.</param>
    /// <param name="options">Options du backend ; le dictionnaire est copié avec comparaison ordinale.</param>
    /// <param name="input">Associations d’entrées, conservées avec leurs collections.</param>
    /// <param name="audioEnabled">Activation de la sortie audio de la machine.</param>
    /// <param name="folders">Chemins configurés pour les données de la machine.</param>
    [JsonConstructor]
    public MachineConfiguration(int schemaVersion, Guid id, MachineModel model, Emulator core, IReadOnlyList<FirmwareConfiguration> firmwares,
        IReadOnlyList<MediaConfiguration> media, IReadOnlyDictionary<string, string> options, InputConfiguration input, bool audioEnabled, FolderConfiguration folders)
        : this(model, firmwares, media, options, input, id, schemaVersion, audioEnabled, folders, core) { }

    /// <summary>Prépare une configuration validée, avec des valeurs initiales pour les éléments facultatifs.</summary>
    /// <param name="model">Modèle initial, que l’adaptateur peut préciser selon les options.</param>
    /// <param name="firmwares">Firmwares de départ ; null produit une liste vide.</param>
    /// <param name="media">Montages de départ ; null produit une liste vide.</param>
    /// <param name="options">Options explicites, prioritaires sur les valeurs initiales du backend ; null crée un dictionnaire vide à compléter.</param>
    /// <param name="input">Associations d’entrées ; null construit InputConfiguration avec ses valeurs initiales.</param>
    /// <param name="id">Identité persistante ; un GUID vide demande une nouvelle identité.</param>
    /// <param name="schemaVersion">Version de schéma contrôlée par la validation Common.</param>
    /// <param name="audioEnabled">Indique si la machine doit activer sa sortie audio.</param>
    /// <param name="folders">Emplacements configurés ; null construit un ensemble de chemins non précisés.</param>
    /// <param name="core">Backend préféré ; null utilise le choix du module pour le modèle.</param>
    /// <remarks>Si le backend choisi ne correspond pas au modèle effectif, le choix par défaut est résolu à nouveau.
    /// Les fichiers référencés par les médias et firmwares ne deviennent pas la propriété de cette configuration.</remarks>
    public MachineConfiguration(MachineModel model, IReadOnlyList<FirmwareConfiguration>? firmwares = null, IReadOnlyList<MediaConfiguration>? media = null,
        IReadOnlyDictionary<string, string>? options = null, InputConfiguration? input = null, Guid id = default, int schemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion,
        bool audioEnabled = true, FolderConfiguration? folders = null, Emulator? core = null)
    {
        Core = core ?? ConfigurationFunctions.DefaultEmulator(model);
        if (Enum.IsDefined(Core) && Core != Emulator.None && new Engine().Adapter(Core) is IEmulatorConfigurationOptions modelOptions)
            model = modelOptions.ResolveModel(model, options ?? new Dictionary<string, string>());
        Model = model;
        if (Enum.IsDefined(Core) && Core != Emulator.None && !EmulatorCatalog.Get(Core).MachineIds.Contains(model.ToString()))
            Core = ConfigurationFunctions.DefaultEmulator(model);
        Firmwares = firmwares?.ToArray() ?? [];
        Media = media?.ToArray() ?? [];
        var selectedOptions = options is null ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(options, StringComparer.Ordinal);
        if (Core != Emulator.None && new Engine().Adapter(Core) is IEmulatorConfigurationOptions defaults)
            foreach (var option in defaults.GetDefaultOptions(model)) selectedOptions.TryAdd(option.Key, option.Value);
        Options = selectedOptions;
        Input = input ?? new InputConfiguration();
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SchemaVersion = schemaVersion;
        AudioEnabled = audioEnabled;
        Folders = folders ?? new FolderConfiguration();
        ConfigurationFunctions.Validate(this);
    }

    /// <summary>Identité technique du module qui sait interpréter cette configuration.</summary>
    public string ModuleId => MachineConfigurationConstants.ModuleId;
    /// <summary>Version du schéma de configuration, indépendante de la version du cœur.</summary>
    public int SchemaVersion { get; init; }
    /// <summary>Identité persistante de cette configuration dans le magasin de machines.</summary>
    public Guid Id { get; init; }
    /// <summary>Modèle matériel effectif, fixé après résolution des options pendant la construction.</summary>
    public MachineModel Model { get; }
    /// <summary>Identifiant de modèle exposé à l’hôte, obtenu depuis son nom technique.</summary>
    public string MachineId => Model.ToString();
    /// <summary>Famille regroupant les modèles décrits par cette configuration.</summary>
    public MachineFamily Family => MachineFamily.ExampleMachineA;
    /// <summary>Backend sélectionné pour exécuter ce modèle.</summary>
    public Emulator Core { get; init; }
    /// <summary>Firmwares sélectionnés et leurs emplacements fonctionnels.</summary>
    public IReadOnlyList<FirmwareConfiguration> Firmwares { get; init; }
    /// <summary>Montages de médias de départ, dans l’ordre de la configuration.</summary>
    public IReadOnlyList<MediaConfiguration> Media { get; init; }
    /// <summary>Options techniques du backend, incluant les valeurs initiales ajoutées lorsqu’elles étaient absentes.</summary>
    public IReadOnlyDictionary<string, string> Options { get; init; }
    /// <summary>Associations de clavier, souris et contrôleurs utilisées pour projeter les entrées de l’hôte.</summary>
    public InputConfiguration Input { get; init; }
    /// <summary>Activation de la sortie audio, distincte du volume configuré.</summary>
    public bool AudioEnabled { get; init; }
    /// <summary>Emplacements des données de la machine, résolus par les services concernés.</summary>
    public FolderConfiguration Folders { get; init; }
}
