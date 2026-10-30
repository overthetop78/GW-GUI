using System.Buffers;

namespace GWGUI.Emulation;

/// <summary>Accès aux services du SDK par leurs contrats publics, sans connaissance de l’identité des modules.</summary>
public static class EmulationSdk
{
    /// <summary>Préparations SCP/CUE et données de formats par contrat public, avec MediaEngine comme propriétaire des traitements.</summary>
    public static IEmulationMediaProcessing MediaProcessing { get; } = new EmulationMediaProcessingService();

    /// <summary>Descriptions et projection des retours des cœurs vers les associations de sortie, sans périphérique possédé.</summary>
    public static IEmulationControllerFeedbackProcessing ControllerFeedback { get; } = new EmulationControllerFeedbackService();

    /// <summary>Règles et documents de montages par contrat public, sans accès aux implémentations internes.</summary>
    public static IEmulationMediaRules MediaRules { get; } = new EmulationMediaRules();

    /// <summary>Parcours des fichiers de firmware sous leur contrat public, avec identification laissée au module.</summary>
    public static IEmulationFirmwareProcessing FirmwareProcessing { get; } = new EmulationFirmwareFunctions();

    /// <summary>Copie, géométrie et métadonnées vidéo par contrat public, sans état de surface partagé.</summary>
    public static IEmulationVideoProcessing VideoProcessing { get; } = new EmulationVideoProcessingService();

    /// <summary>Construction et formatage des réglages matériels et audio sous leur contrat public commun.</summary>
    public static IEmulationHardwareSettings HardwareSettings { get; } = new EmulationHardwareSettingsService();

    /// <summary>Traitements d’environnement des cœurs natifs par contrat public, avec les politiques et propriétaires fournis par Common.</summary>
    public static IEmulationCoreEnvironmentProcessing EnvironmentProcessing { get; } = new EmulationCoreEnvironmentService();

    /// <summary>Traitement des options natives et de leurs champs de réglage, sous son contrat public sans état de session.</summary>
    public static IEmulationCoreOptionProcessing OptionProcessing { get; } = new EmulationCoreOptionService();

    /// <summary>Allocations natives par contrat public ; le service est emprunté, les résultats sont possédés par leurs appelants.</summary>
    public static IEmulationNativeResources NativeResources { get; } = new EmulationNativeResourceService();

    /// <summary>Découverte des adaptateurs et fabriques d’un module par leurs contrats, sans liste de modules connus.</summary>
    public static IEmulationAdapters Adapters { get; } = new EmulationAdapterService();

    /// <summary>Service d’inspection sans état partagé mutable et sans bibliothèque native chargée.</summary>
    /// <remarks>L’implémentation reste interne au SDK. Le module utilise le contrat et n’a aucun service à libérer.</remarks>
    public static IEmulationCoreDiagnostics CoreDiagnostics { get; } = new EmulationCoreDiagnosticFunctions();

    /// <summary>Sélection et installation des versions par le contrat commun, sans accès à la classe interne.</summary>
    /// <remarks>Ce service sans état ne possède pas le client HTTP ni les callbacks fournis par le module.</remarks>
    public static IEmulationCoreReleases CoreReleases { get; } = new EmulationCoreReleaseFunctions();

    /// <summary>Traitements audio sans état ; sorties et pointeurs restent possédés par les appelants.</summary>
    public static IEmulationAudioProcessing AudioProcessing { get; } = new EmulationAudioFunctions();

    /// <summary>Nettoyage des seuls dossiers de session possédés, après destruction des cœurs.</summary>
    public static IEmulationSessionDirectories SessionDirectories { get; } = new EmulationSessionDirectoryFunctions();

    /// <summary>Format de sauvegarde commun, sans accès aux lecteurs binaires internes.</summary>
    public static IEmulationStateFiles StateFiles { get; } = new EmulationStateFileService();

    /// <summary>Persistance par contrat, avec les propriétaires d’écriture et de verrouillage existants.</summary>
    public static IEmulationConfigurationFiles ConfigurationFiles { get; } = new EmulationConfigurationFileService();

    /// <summary>Échanges binaires entre Common et son hôte par leur contrat public, avec les codecs internes existants.</summary>
    public static IEmulationHostProtocol HostProtocol { get; } = new EmulationHostProtocolService();

    /// <summary>Copie stable des entrées par contrat public, sans accès aux fonctions internes de mappage.</summary>
    public static IEmulationInputProcessing InputProcessing { get; } = new EmulationInputProcessingService();

    /// <summary>Cycle de vie, commandes et publication des machines sous leur contrat d’exécution commun.</summary>
    public static IEmulationMachineExecution MachineExecution { get; } = new EmulationMachineExecutionService();

    /// <summary>Présence et compatibilité des interfaces ; registre en mémoire sans conversion de réseau.</summary>
    public static IEmulationNetworks Networks { get; } = new EmulationNetworkService();

    /// <summary>Contextualisation des messages sans accès aux services d’implémentation.</summary>
    public static IEmulationMessages Messages { get; } = new EmulationMessageService();

    /// <summary>Crée le tampon audio borné existant sous son interface, sans accès à sa classe interne.</summary>
    /// <returns>Tampon indépendant dont la session propriétaire vide les blocs lors de son nettoyage.</returns>
    public static IEmulationAudioBuffer CreateAudioBuffer() => new EmulationAudioBuffer();

    /// <summary>Fournit le contrôleur existant sous son contrat ; la sortie reçue reste empruntée.</summary>
    public static IEmulationAudioOutputController CreateAudioOutputController(IAudioOutput? output = null,
        Func<IAudioOutput?>? factory = null) => new EmulationAudioOutputController(output, factory);

    /// <summary>Crée le propriétaire de mémoire vidéo partagée existant sous son interface publique.</summary>
    /// <param name="namePrefix">Préfixe unique de la session fourni par le module.</param>
    /// <param name="capacity">Règle empruntée calculant la capacité d’un emplacement à partir d’une longueur d’image en octets.</param>
    /// <param name="separator">Séparateur invariant entre le préfixe et l’identité d’une génération de segment.</param>
    /// <param name="nameFormat">Format du GUID employé pour nommer les générations.</param>
    /// <returns>Rédacteur possédé par l’appelant, à libérer après la fin des écritures de session.</returns>
    public static IEmulationSharedVideoWriter CreateSharedVideoWriter(string namePrefix, Func<int, int> capacity, string separator, string nameFormat) =>
        new EmulationSharedVideoWriter(namePrefix, capacity, separator, nameFormat);

    /// <summary>Crée le jeu tournant de buffers vidéo existant sous son interface publique.</summary>
    /// <param name="bufferCount">Nombre d’emplacements de roulement choisi par le module.</param>
    /// <param name="pool">Pool emprunté, ou null pour le pool partagé ; le jeu possède ses locations et non le pool.</param>
    /// <returns>Jeu possédé par l’appelant, à libérer pour restituer les tableaux loués.</returns>
    public static IEmulationVideoBufferSet CreateVideoBufferSet(int bufferCount, ArrayPool<byte>? pool = null) => new EmulationVideoBufferSet(bufferCount, pool);
}
