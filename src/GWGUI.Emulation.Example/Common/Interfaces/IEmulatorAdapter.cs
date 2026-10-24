using CommonFirmwareProcessing = GWGUI.Emulation.Example.Common.Functions.CoreHostFunctions;
using CommonAudioFunctions = GWGUI.Emulation.Example.Common.Functions.AudioFunctions;
using System.IO;
using CommonControllerFunctions = GWGUI.Emulation.Example.Common.Functions.ControllerFunctions;
namespace GWGUI.Emulation.Example.Common.Interfaces;
/// <summary>Contrat Common d’un backend : gestion de son installation, projection des médias et création de machines.</summary>
/// <remarks>Le contrat SDK hérité fournit les identifiants, la description, les commandes d’hôte et les opérations
/// de recherche et d’installation. Create reçoit les services empruntés et retourne une machine Common possédant
/// son cœur. La fabrique libère le cœur dans finally si sa construction échoue avant ce transfert de propriété.</remarks>
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorCapabilities, IEmulatorConfigurationSelection, IEmulatorConfigurationValidation,
    IEmulatorControllerAdapter, IEmulatorFirmwareAdapter, IEmulatorConfigurationOptions, IEmulatorContentFormats, IEmulatorControllerFeedbackAdapter,
    IEmulatorInitialContentSettings, IEmulatorPersistentStorage, IEmulatorRuntimePolicy, IEmulatorSettings, IEmulatorSettingsApplication, IEmulatorStorageSettings, IEmulatorVideoSettings, IEmulatorPointerCapabilities, IEmulatorMachineDefaults
{
    /// <summary>Sélectionne la commande native lorsque le profil ne déclare aucune combinaison de touches.</summary>
    IReadOnlySet<EmulationKey> IEmulatorRuntimePolicy.SoftResetKeys => new HashSet<EmulationKey>();

    /// <summary>Conserve les annonces du runtime lorsqu'aucun catalogue préalable n'est déclaré par le backend.</summary>
    /// <param name="configuration">Configuration empruntée à décrire, ou null pour consulter les réglages initiaux du modèle ou du catalogue.</param>
    /// <param name="context">Contexte emprunté apportant les services et les chemins de gestion ; son client HTTP reste possédé par l&apos;appelant.</param>
    /// <returns>Options décrites avant le lancement du cœur ; liste vide si l&apos;adaptateur utilise uniquement les annonces du runtime.</returns>
    IReadOnlyList<CoreOption> IEmulatorSettings.GetSettingsOptions(MachineConfiguration? configuration, EmulatorManagementContext? context) => [];

    /// <summary>Conserve les observations initiales lorsque le backend ne fournit aucune règle de reconnaissance.</summary>
    /// <param name="firmware">Observation initiale du fichier, avec sa description typée et ses caractéristiques déjà inspectées.</param>
    /// <returns>Null avec cette implémentation de base, qui ne remplace pas l&apos;observation initiale.</returns>
    EmulationFirmwareObservation<TDefinition>? IEmulatorFirmwareAdapter.IdentifyFirmware<TDefinition>(EmulationFirmwareObservation<TDefinition> firmware) => null;

    /// <summary>Résout le besoin global depuis les emplacements obligatoires, sans choisir une alternative native.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>True si au moins un emplacement de firmware du contrat est obligatoire.</returns>
    bool IEmulatorFirmwareAdapter.RequiresExternalFirmware(MachineConfiguration configuration) => ((IEmulatorFirmwareAdapter)this).GetFirmwareSlots(configuration).Any(slot => slot.IsRequired);

    /// <summary>Réutilise l'inspection native pour le champ demandé sans déduire de rôle depuis le nom de fichier.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="fieldId">Identifiant technique du champ de configuration ou de destination du firmware.</param>
    /// <param name="path">Chemin emprunté du fichier à inspecter ou du média proposé ; l&apos;appel ne transfère pas sa propriété.</param>
    /// <returns>True lorsqu&apos;une inspection compatible propose le champ de destination demandé.</returns>
    bool IEmulatorFirmwareAdapter.IsFirmwareCompatible(MachineConfiguration configuration, string fieldId, string path)
    {
        var firmware = (IEmulatorFirmwareAdapter)this;
        return firmware.GetFirmwareSlots(configuration).Any(slot => slot.FieldId == fieldId)
            && firmware.ScanFirmwareFileAsync(configuration, path, CancellationToken.None).GetAwaiter().GetResult()
                .Any(candidate => candidate.DestinationFieldId == fieldId && candidate.Compatibility != EmulationFirmwareCompatibility.Incompatible);
    }

    /// <summary>Réutilise le parcours plat du service Common et l'inspecteur natif sans modifier ses résultats.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="directory">Dossier source à parcourir pour inspecter les fichiers de firmware, sans modification de leurs contenus.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente de la liste des fichiers candidats et de leurs destinations compatibles, sans installation dans une session.</returns>
    async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> IEmulatorFirmwareAdapter.ScanFirmwareAsync(MachineConfiguration configuration, string directory, CancellationToken cancellationToken)
    {
        var candidates = await CommonFirmwareProcessing.FirmwareProcessing.ScanDirectoryAsync(directory,
            (path, token) => ((IEmulatorFirmwareAdapter)this).ScanFirmwareFileAsync(configuration, path, token), cancellationToken).ConfigureAwait(false);
        return candidates.SelectMany(candidateSet => candidateSet).ToArray();
    }

    /// <summary>Reprend les blocs natifs dans leur portée de présentation existante.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="context">Contexte emprunté apportant les services et les chemins de gestion ; son client HTTP reste possédé par l&apos;appelant.</param>
    /// <returns>Blocs de réglages fournis par le contrat de description de l&apos;adaptateur.</returns>
    IReadOnlyList<EmulationSettingsBlock> IEmulatorSettings.GetMachineSettingsBlocks(MachineConfiguration configuration, EmulatorManagementContext? context) => ((IEmulatorSettings)this).DescribeSettings(configuration, context);

    /// <summary>Conserve les champs propres au périphérique lorsque le backend en fournit.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="category">Catégorie de stockage dont les champs doivent être décrits.</param>
    /// <returns>Champs particuliers de cette catégorie de stockage ; liste vide sans champ supplémentaire.</returns>
    IReadOnlyList<EmulationSettingsField> IEmulatorStorageSettings.GetStorageDeviceConfigurationFields(MachineConfiguration configuration, EmulationMediaCategory category) => [];


    /// <summary>Conserve la capacité du catalogue et les réglages de RAM déjà décrits par la famille.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Capacité de mémoire principale du modèle en kibioctets, ou null si elle n&apos;est pas annoncée.</returns>
    decimal? IEmulatorSettings.GetMemoryKib(MachineConfiguration configuration) => HardwareModelCatalog.Get(configuration.Model)?.MainMemoryBytes / 1024m;


    /// <summary>Conserve la résolution des valeurs audio déjà utilisée par la famille.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Description du périphérique de restitution et de la latence en millisecondes, sans ouverture de sortie audio.</returns>
    AudioConfiguration IEmulatorSettings.GetAudioConfiguration(MachineConfiguration configuration) => new(
        CommonAudioFunctions.Processing.ResolveRuntimeOutput(configuration.Options, SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput),
        CommonAudioFunctions.Processing.ResolveRuntimeLatency(configuration.Options, SettingsConstants.AudioLatency));

    /// <summary>Conserve les choix de latence existants, remplacés par les contraintes du cœur lorsqu’il les redéfinit.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Liste partagée des latences proposées, exprimées en millisecondes.</returns>
    IReadOnlyList<int> IEmulatorSettings.GetAudioLatencyChoices(MachineConfiguration configuration) => AudioConstants.LatencyChoicesMilliseconds;


    /// <summary>Conserve les options lorsque le cœur ne possède aucune conversion du choix graphique de l'hôte.</summary>
    /// <remarks>La présentation des images reste sous la responsabilité de l'hôte ; cette opération ne démarre aucun cœur et ne possède aucune surface graphique.</remarks>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="graphicsApi">API graphique choisie par l&apos;hôte ; cet argument ne transfère aucune fenêtre ni surface possédée.</param>
    /// <returns>Configuration retournée par la politique vidéo de l&apos;adaptateur, inchangée lorsqu&apos;aucune adaptation n&apos;est nécessaire.</returns>
    MachineConfiguration IEmulatorVideoSettings.ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi) => configuration;

    /// <summary>Reprend les champs de fichiers de firmware déjà décrits lorsque le backend ne fournit pas de catalogue explicite.</summary>
    /// <remarks>Le nom de destination et l'obligation ne sont pas déduits du libellé. Les champs d'information,
    /// les sélecteurs de modèles et les dossiers ne sont pas des emplacements de fichiers de firmware.</remarks>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Descriptions des emplacements de fichiers de firmware, avec leurs champs de destination.</returns>
    IReadOnlyList<FirmwareSlot> IEmulatorFirmwareAdapter.GetFirmwareSlots(MachineConfiguration configuration) => ((IEmulatorSettings)this).DescribeSettings(configuration)
        .SelectMany(block => block.Fields).Where(field => field.Editor == EmulationSettingsEditor.Path && (field.Tab == EmulationMachineTab.Rom || field.DefaultFolderCategory == EmulationDefaultFolderCategory.Firmware))
        .GroupBy(field => field.Id, StringComparer.Ordinal).Select(group => group.First())
        .Select(field => new FirmwareSlot(field.Id, string.Empty, field.LabelResourceKey, ExplanationResourceKey: field.ExplanationResourceKey, DetailedExplanationResourceKey: field.DetailedExplanationResourceKey)).ToArray();

    /// <summary>Prépare le dossier système quand aucun traitement natif de firmware ne le complète.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="systemDirectory">Dossier système préparé pour les firmwares de la session ; son nettoyage appartient au propriétaire de cette session.</param>
    void IEmulatorFirmwareAdapter.PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => Directory.CreateDirectory(systemDirectory);

    /// <summary>Conserve la validation habituelle lorsque le cœur ne déclare aucune option différée.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="key">Identifiant technique de l&apos;option du cœur à consulter ou à modifier.</param>
    /// <returns>False avec la règle de base : aucune option n&apos;est dispensée du parcours habituel de validation.</returns>
    bool IEmulatorSettings.IsDeferredOption(MachineConfiguration configuration, string key) => false;

    /// <summary>Conserve la description du champ lorsque le cœur ne fournit aucune reconnaissance particulière de son horloge CPU.</summary>
    /// <param name="fieldId">Identifiant technique du champ de configuration ou de destination du firmware.</param>
    /// <returns>False avec la règle de base, qui ne reconnaît aucun champ spécifique de fréquence CPU.</returns>
    bool IEmulatorSettings.IsCpuClock(string fieldId) => false;

    /// <summary>Conserve le choix initial du module tant que le cœur ne déclare aucune recommandation.</summary>
    /// <param name="machineId">Identifiant stable du profil matériel, résolu et validé par le catalogue Common.</param>
    /// <returns>False avec la règle de base, qui ne déclare aucune recommandation supplémentaire.</returns>
    bool IEmulatorMachineDefaults.IsDefaultFor(string machineId) => false;

    /// <summary>Conserve la sélection séparée désactivée tant que le cœur ne la déclare pas.</summary>
    bool IEmulatorInitialContentSettings.SupportsInitialContent => false;

    /// <summary>Réutilise les médias configurés et conserve uniquement les sources insérées dans leur ordre de lecteurs.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Chemins des médias configurés et insérés, dans l&apos;ordre retenu par la description des lecteurs.</returns>
    IReadOnlyList<string> IEmulatorInitialContentSettings.ResolveInitialContentPaths(MachineConfiguration configuration)
        => ResolveConfiguredMedia(configuration).Where(media => media.IsInserted).OrderBy(media => media.Slot).Select(media => media.Path).ToArray();

    /// <summary>Conserve l’absence de valeurs initiales propres au cœur ; les catalogues natifs peuvent fournir leur implémentation.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Dictionnaire des valeurs initiales propres à l&apos;adaptateur ; vide avec l&apos;implémentation de base.</returns>
    IReadOnlyDictionary<string, string> IEmulatorConfigurationOptions.GetDefaultOptions(MachineConfiguration configuration) => new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Complète les valeurs manquantes sans écraser celles préparées par l’appelant.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="options">Dictionnaire modifiable des options à compléter ; les valeurs déjà présentes ne sont pas remplacées par les valeurs initiales.</param>
    void IEmulatorConfigurationOptions.InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options)
    {
        foreach (var option in ((IEmulatorConfigurationOptions)this).GetDefaultOptions(configuration))
            options.TryAdd(option.Key, option.Value);
    }

    /// <summary>Réutilise le contrôle de configuration du cœur lorsque celui-ci ne sépare pas la validation des options.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    void IEmulatorConfigurationOptions.ValidateOptions(MachineConfiguration configuration) => ((IEmulatorConfigurationValidation)this).ValidateConfiguration(configuration);

    /// <summary>Conserve l’identité du modèle en l’absence de résolution d’une variante native.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Modèle courant, sans substitution de variante par l&apos;implémentation de base.</returns>
    MachineModel IEmulatorConfigurationOptions.ResolveModel(MachineConfiguration configuration) => configuration.Model;

    /// <summary>Conserve les options empruntées lorsqu’aucune normalisation native n’est nécessaire.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Dictionnaire d&apos;options courant, conservé sans modification par la règle de base.</returns>
    IReadOnlyDictionary<string, string> IEmulatorConfigurationOptions.NormalizeOptions(MachineConfiguration configuration) => configuration.Options;

    /// <summary>Réutilise les commandes décrites dans Common ; les cœurs émulant des touches conservent leur adaptation.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="port">Index technique du port, à partir de zéro, vérifié selon les capacités de la configuration.</param>
    /// <param name="peripheral">Catégorie de périphérique à décrire ou à appliquer au port.</param>
    /// <returns>Définitions des commandes du périphérique dans la configuration du port.</returns>
    IReadOnlyList<InputActionDefinition> IEmulatorControllerAdapter.DescribeInput(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => CommonControllerFunctions.InputProcessing.DescribeInput(InputSettingsFunctions.Describe(configuration), port, peripheral.ToString());

    /// <summary>Réutilise les définitions du choix existant sans recréer les libellés ni les directions opposées.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="peripheral">Catégorie de périphérique à décrire ou à appliquer au port.</param>
    /// <param name="port">Index technique du port, à partir de zéro, vérifié selon les capacités de la configuration.</param>
    /// <returns>Définitions des associations du périphérique et du port, ou null lorsque le contrat ne fournit pas de liste spécifique.</returns>
    IReadOnlyList<InputBindingDefinition>? IEmulatorControllerAdapter.GetControllerBindingDefinitions(MachineConfiguration configuration, PeripheralCategory peripheral, int port) => CommonControllerFunctions.InputProcessing.GetControllerBindingDefinitions(InputSettingsFunctions.Describe(configuration), peripheral.ToString(), port);

    /// <summary>Expose les ports matériels lorsque le cœur ne fournit aucune extension propre.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Nombre de ports annoncé par les règles matérielles de ce contrat.</returns>
    int IEmulatorControllerAdapter.GetControllerPortCount(MachineConfiguration configuration) => CommonControllerFunctions.ModelPortCount(configuration);

    /// <summary>Conserve le nom déjà décrit pour le port ; un cœur peut fournir son organisation particulière.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="port">Index technique du port, à partir de zéro, vérifié selon les capacités de la configuration.</param>
    /// <returns>Nom de présentation du port, ou null pour laisser l&apos;hôte utiliser son libellé commun.</returns>
    string? IEmulatorControllerAdapter.GetControllerPortDisplayName(MachineConfiguration configuration, int port) => CommonControllerFunctions.InputProcessing.GetControllerPort(InputSettingsFunctions.Describe(configuration), port)?.DisplayName;

    /// <summary>Conserve le numéro de présentation déjà décrit pour le port.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="port">Index technique du port, à partir de zéro, vérifié selon les capacités de la configuration.</param>
    /// <returns>Numéro de présentation du port, distinct de son index technique, ou null sans numéro spécifique.</returns>
    int? IEmulatorControllerAdapter.GetControllerPortDisplayNumber(MachineConfiguration configuration, int port) => CommonControllerFunctions.InputProcessing.GetControllerPort(InputSettingsFunctions.Describe(configuration), port)?.DisplayNumber;

    /// <summary>Consulte les choix matériels existants lorsque le cœur ne fournit aucune liste propre.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Catégories de périphériques autorisées dans la portée de la demande.</returns>
    IReadOnlyList<PeripheralCategory> IEmulatorControllerAdapter.GetPeripherals(MachineConfiguration configuration) => CommonControllerFunctions.InputProcessing.GetPeripherals<PeripheralCategory>(InputSettingsFunctions.Describe(configuration));

    /// <summary>Consulte les choix existants du port demandé sans les mélanger avec ceux des autres ports.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="port">Index technique du port, à partir de zéro, vérifié selon les capacités de la configuration.</param>
    /// <returns>Catégories de périphériques autorisées dans la portée de la demande.</returns>
    IReadOnlyList<PeripheralCategory> IEmulatorControllerAdapter.GetPeripherals(MachineConfiguration configuration, int port) => CommonControllerFunctions.InputProcessing.GetPeripherals<PeripheralCategory>(InputSettingsFunctions.Describe(configuration), port);

    /// <summary>Consulte les ports USB déjà décrits ; ne crée aucun bus ou périphérique absent du modèle.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="port">Index technique du port, à partir de zéro, vérifié selon les capacités de la configuration.</param>
    /// <returns>Catégories de périphériques USB décrites pour ce port.</returns>
    IReadOnlyList<PeripheralCategory> IEmulatorControllerAdapter.GetUsbPeripherals(MachineConfiguration configuration, int port) => CommonControllerFunctions.InputProcessing.GetPeripherals<PeripheralCategory>(InputSettingsFunctions.Describe(configuration), port, usb: true);


    /// <summary>Décrit les deux moteurs standard pour les catégories valides autres que None et Keyboard.</summary>
    /// <param name="configuration">Configuration empruntée du modèle et des ports.</param>
    /// <param name="peripheral">Catégorie du périphérique émulé à décrire.</param>
    /// <returns>Définitions communes ; les implémentations concrètes peuvent préciser les capacités de leur backend.</returns>
    /// <remarks>Cette description ne démarre aucun effet et ne possède aucun périphérique physique.</remarks>
    IReadOnlyList<EmulationControllerOutputDefinition> IEmulatorControllerFeedbackAdapter.GetControllerOutputs(MachineConfiguration configuration, PeripheralCategory peripheral) => CommonControllerFunctions.Feedback.DescribeOutputs(peripheral);

    /// <summary>Conserve les réglages de présentation vidéo communs tant que le backend ne déclare aucune restriction.</summary>
    bool IEmulatorCapabilities.SupportsVideoSettings => true;

    /// <summary>Ne demande aucune recréation sans média tant que le backend ne fournit pas cette garantie.</summary>
    /// <remarks>Cette déclaration reste distincte de la capacité annoncée par les callbacks natifs après initialisation.</remarks>
    bool IEmulatorCapabilities.SupportsNoContent => false;

    /// <summary>Laisse le cœur annoncer ses extensions lorsque le catalogue ne fournit aucune liste statique.</summary>
    /// <remarks>Une liste vide ne refuse aucun format : la déclaration native reste disponible au runtime.</remarks>
    IReadOnlyList<string> IEmulatorContentFormats.SupportedContentExtensions => [];

    /// <summary>Conserve les extensions natives sans modifier la collection empruntée lorsque le backend ne demande aucune adaptation.</summary>
    /// <param name="declaredExtensions">Ensemble annoncé par le cœur, dont le comparateur et les valeurs sont conservés.</param>
    /// <param name="requiresFilePath">Contrainte native disponible pour les adaptations concrètes.</param>
    /// <returns>La même instance de collection, sans copie ni ajout de format.</returns>
    IReadOnlySet<string> IEmulatorContentFormats.ResolveContentExtensions(IReadOnlySet<string> declaredExtensions, bool requiresFilePath) => declaredExtensions;

    /// <summary>Ne déclare aucun fichier associé lorsque le backend ne décrit pas de dépendance de média.</summary>
    /// <param name="path">Chemin emprunté ; ce comportement ne le consulte pas et ne modifie aucun fichier.</param>
    /// <returns>Liste vide avec cette implémentation de base, qui ne déclare aucun fichier associé.</returns>
    IReadOnlyList<string> IEmulatorContentFormats.GetMediaDependencies(string path) => [];

    /// <summary>Conserve les fichiers persistants existants si le backend ne demande aucune préparation supplémentaire.</summary>
    /// <param name="configuration">Configuration empruntée décrivant les supports de la machine.</param>
    /// <param name="directory">Dossier persistant fourni par Common, distinct du dossier temporaire de session.</param>
    /// <remarks>Ce comportement ne crée ni dossier ni fichier. Une implémentation concrète prépare uniquement les données requises par son cœur.</remarks>
    void IEmulatorPersistentStorage.EnsurePersistentStorage(MachineConfiguration configuration, string directory) { }

    /// <summary>Conserve la configuration préparée par Common si le backend ne demande aucune adaptation lors de sa sélection.</summary>
    /// <param name="configuration">Configuration empruntée portant le cœur choisi et les données existantes.</param>
    /// <returns>La même instance ; une implémentation concrète conserve les données qui ne concernent pas ses contraintes.</returns>
    MachineConfiguration IEmulatorConfigurationSelection.SelectConfiguration(MachineConfiguration configuration) => configuration;

    /// <summary>Conserve les réglages préparés par Common lorsque le backend ne demande aucune adaptation supplémentaire.</summary>
    /// <param name="configuration">Configuration empruntée après les traitements généraux du module.</param>
    /// <param name="values">Champs proposés, disponibles pour les adaptations concrètes.</param>
    /// <returns>La même instance sans adaptation ; les implémentations concrètes retournent leur résultat sans modifier la source.</returns>
    MachineConfiguration IEmulatorSettingsApplication.ApplySettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values) => configuration;

    /// <summary>Conserve les dimensions matérielles de Common lorsque le backend ne fournit aucune résolution configurée.</summary>
    /// <param name="configuration">Configuration empruntée à consulter par les spécialisations concrètes.</param>
    /// <returns>Null sans annonce propre au backend ; une implémentation concrète retourne sa taille en pixels.</returns>
    (int Width, int Height)? IEmulatorVideoSettings.GetOutputResolution(MachineConfiguration configuration) => null;

    /// <summary>Ne demande par défaut aucun redémarrage supplémentaire pour un changement de média.</summary>
    /// <remarks>Les règles générales de Common et la politique concrète du backend restent responsables des décisions effectives.</remarks>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="slot">Emplacement du média à consulter ou à commander, selon son identité de protocole.</param>
    /// <param name="path">Chemin emprunté du fichier à inspecter ou du média proposé ; l&apos;appel ne transfère pas sa propriété.</param>
    /// <returns>False avec la règle de base, qui n&apos;ajoute pas de redémarrage à la politique générale de médias.</returns>
    bool IEmulatorRuntimePolicy.RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => false;

    /// <summary>Ne demande par défaut aucun redémarrage supplémentaire pour une option.</summary>
    /// <remarks>Les métadonnées des options et les contraintes concrètes du backend restent appliquées par leur propriétaire.</remarks>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="key">Identifiant technique de l&apos;option du cœur à consulter ou à modifier.</param>
    /// <returns>False avec la règle de base, qui n&apos;ajoute pas de redémarrage aux métadonnées des options.</returns>
    bool IEmulatorRuntimePolicy.RequiresOptionRestart(MachineConfiguration configuration, string key) => false;

    /// <summary>Ne fournit par défaut aucun lecteur supplémentaire propre au backend.</summary>
    /// <param name="configuration">Configuration empruntée dont les descriptions concrètes peuvent dépendre.</param>
    /// <returns>Liste vide lorsque le backend ne complète pas les lecteurs généraux du modèle.</returns>
    /// <remarks>Common conserve ses descriptions matérielles. Une implémentation concrète fournit les lecteurs propres au cœur
    /// par ce contrat ; elle ne possède ni les fichiers référencés ni leurs contenus montés.</remarks>
    IReadOnlyList<EmulationMediaDevice> IEmulatorMediaCapabilities.DescribeMediaDevices(MachineConfiguration configuration) => [];

    /// <summary>Conserve les contrôles généraux de Common lorsque le backend ne déclare aucune contrainte supplémentaire.</summary>
    /// <remarks>La fabrique concrète implémente ce contrat pour ajouter ses contrôles ; une fabrique dérivée redéfinit
    /// le validateur de sa base avec override.
    /// Ce point commun ne modifie ni la configuration ni les contenus ; les contrôles généraux restent exécutés par leur propriétaire.</remarks>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    void IEmulatorConfigurationValidation.ValidateConfiguration(MachineConfiguration configuration) { }

    /// <summary>Annonce par défaut l’absence d’entrées de pointage ; le backend déclare sa prise en charge effective.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>False avec la capacité de base ; un adaptateur concret annonce les entrées de pointage qu&apos;il accepte.</returns>
    bool IEmulatorPointerCapabilities.SupportsPointer(MachineConfiguration configuration) => false;

    /// <summary>Décrit les extensions de cartouche acceptées pour une configuration.</summary>
    /// <param name="configuration">Configuration empruntée précisant le modèle et ses options.</param>
    /// <returns>Ensemble des extensions spécifiques, ou null lorsque l’adaptateur n’en fournit pas de restriction spécifique.</returns>
    IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration);


    /// <summary>Projette les réglages validés du module vers les valeurs textuelles attendues pendant l’exécution.</summary>
    /// <param name="configuration">Configuration de machine, conservée immuable par le module.</param>
    /// <returns>Associations de clés et valeurs techniques ; les nombres utilisent un format indépendant de la langue.</returns>
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);

}
