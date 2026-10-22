using System.Text.Json.Serialization;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

/// <summary>Répertoires personnels d’une configuration, indépendants des dossiers temporaires du cœur.</summary>
/// <param name="Shared">Répertoire partagé avec le système émulé, ou null sans partage.</param>
/// <param name="Floppies">Répertoire initial de sélection des disquettes.</param>
/// <param name="States">Répertoire de sauvegarde des états de la machine.</param>
/// <param name="Captures">Répertoire des captures d’image.</param>
public sealed record FolderConfiguration(string? Shared = null, string? Floppies = null, string? States = null, string? Captures = null);

/// <summary>Configuration persistante commune aux deux machines et aux trois émulateurs fictifs.</summary>
/// <remarks>Le document conserve le modèle, le choix du cœur et les chemins des contenus. Ses collections
/// sont copiées à la construction pour éviter de conserver une liste mutable de l’appelant. Le constructeur
/// ne charge aucun média, ne démarre aucun cœur et ne sélectionne pas silencieusement un autre émulateur.</remarks>
public sealed record MachineConfiguration : IEmulationConfiguration
{
    /// <summary>Construit la configuration à partir des données du document ou d’un choix utilisateur.</summary>
    /// <param name="model">L’un des deux profils matériels du catalogue.</param>
    /// <param name="firmwares">Fichiers de firmware facultatifs, conservés sous la propriété de l’utilisateur.</param>
    /// <param name="media">Contenus montés et éjectés avec leurs emplacements.</param>
    /// <param name="options">Valeurs techniques des réglages du cœur.</param>
    /// <param name="input">Associations des sources physiques aux périphériques émulés.</param>
    /// <param name="id">Identifiant persistant ; un nouvel identifiant est créé si la valeur est vide.</param>
    /// <param name="schemaVersion">Version du document de configuration du module.</param>
    /// <param name="audioEnabled">Autorisation d’émettre l’audio produit par le cœur.</param>
    /// <param name="folders">Répertoires personnels, indépendants du stockage de session.</param>
    /// <param name="core">Émulateur choisi ; le cœur interne constitue la préférence initiale de l’exemple.</param>
    /// <param name="configuredSlots">Lecteurs choisis ; null active les lecteurs du profil, une liste vide conserve une sélection vide.</param>
    [JsonConstructor]
    public MachineConfiguration(MachineModel model, IReadOnlyList<FirmwareConfiguration>? firmwares = null, IReadOnlyList<MediaConfiguration>? media = null,
        IReadOnlyDictionary<string, string>? options = null, InputConfiguration? input = null, Guid id = default,
        int schemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion, bool audioEnabled = true, FolderConfiguration? folders = null, Emulator core = Emulator.Internal, IReadOnlyList<EmulationMediaSlot>? configuredSlots = null)
    {
        Model = model;
        Core = core;
        Firmwares = Array.AsReadOnly(firmwares?.ToArray() ?? []);
        Media = Array.AsReadOnly(media?.ToArray() ?? []);
        ConfiguredSlots = configuredSlots is not null ? Array.AsReadOnly(configuredSlots.ToArray()) : Enum.IsDefined(model) ? ConfigurationFunctions.DefaultSlots(model) : [];
        Options = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(options is null
            ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(options, StringComparer.Ordinal));
        Input = input ?? new InputConfiguration();
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SchemaVersion = schemaVersion;
        AudioEnabled = audioEnabled;
        Folders = folders ?? new FolderConfiguration();
    }

    /// <summary>Identité publique du module, identique à celle annoncée par sa façade.</summary>
    public string ModuleId => EmulationModuleConstants.ModuleId;
    /// <summary>Version du schéma de ce document de configuration.</summary>
    public int SchemaVersion { get; init; }
    /// <summary>Identité stable de l’instance, conservée lors des changements de réglages.</summary>
    public Guid Id { get; init; }
    /// <summary>Profil matériel choisi ; chaque émulateur accepte les deux valeurs.</summary>
    public MachineModel Model { get; init; }
    /// <summary>Identifiant du profil dans le catalogue public, indépendant de sa traduction.</summary>
    public string MachineId => HardwareModelCatalog.Get(Model).StableModelId;
    /// <summary>Famille de présentation commune des profils fictifs.</summary>
    public MachineFamily Family => MachineFamily.Example;
    /// <summary>Identité du cœur choisi ; None représente l’absence de choix.</summary>
    public Emulator Core { get; init; }
    /// <summary>Fichiers de firmware décrits sans conserver de flux ouverts.</summary>
    public IReadOnlyList<FirmwareConfiguration> Firmwares { get; init; }
    /// <summary>Liste des contenus montés ou éjectés, possédés par leur appelant.</summary>
    public IReadOnlyList<MediaConfiguration> Media { get; init; }
    /// <summary>Lecteurs configurés, indépendamment des médias actuellement montés.</summary>
    /// <remarks>La construction copie le choix de l’appelant. Une liste vide signifie qu’aucun lecteur
    /// n’est configuré ; elle reste distincte du choix initial qui active les lecteurs du profil matériel.</remarks>
    public IReadOnlyList<EmulationMediaSlot> ConfiguredSlots { get; init; }
    /// <summary>Options du backend indexées par clé technique ordinale.</summary>
    public IReadOnlyDictionary<string, string> Options { get; init; }
    /// <summary>Associations d’entrées persistantes de cette configuration.</summary>
    public InputConfiguration Input { get; init; }
    /// <summary>Autorisation de sortie audio ; le cœur peut continuer à calculer ses trames lorsqu’elle est coupée.</summary>
    public bool AudioEnabled { get; init; }
    /// <summary>Répertoires de travail personnels de l’utilisateur.</summary>
    public FolderConfiguration Folders { get; init; }
}
