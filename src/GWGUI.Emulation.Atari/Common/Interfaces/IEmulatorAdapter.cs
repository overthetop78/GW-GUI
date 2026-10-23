using System.IO;
using CommonControllerFunctions = GWGUI.Emulation.Atari.Common.Functions.ControllerFunctions;
namespace GWGUI.Emulation.Atari.Common.Interfaces;
internal interface IEmulatorAdapter : IEmulatorControllerFeedbackAdapter, IEmulatorCapabilities, IEmulatorContentFormats, IEmulatorPersistentStorage, IEmulatorConfigurationSelection, IEmulatorFirmwareAdapter, GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorRuntimePolicy, IEmulatorMediaCapabilities, IEmulatorConfigurationValidation, IEmulatorSettings, IEmulatorInputMapper, IEmulatorPointerCapabilities, IEmulatorVideoSettings, IEmulatorSettingsApplication, IEmulatorStorageSettings, IEmulatorControllerAdapter, IEmulatorConfigurationOptions, IEmulatorInitialContentSettings, IEmulatorMachineDefaults
{
    /// <summary>Reprend les blocs natifs dans leur portée de présentation existante.</summary>
    IReadOnlyList<EmulationSettingsBlock> IEmulatorSettings.GetMachineSettingsBlocks(MachineConfiguration configuration, EmulatorManagementContext? context) => ((IEmulatorSettings)this).DescribeSettings(configuration, context);

    /// <summary>Conserve les champs propres au périphérique lorsque le backend en fournit.</summary>
    IReadOnlyList<EmulationSettingsField> IEmulatorStorageSettings.GetStorageDeviceConfigurationFields(MachineConfiguration configuration, EmulationMediaCategory category) => [];


    /// <summary>Conserve la capacité du catalogue et les réglages de RAM déjà décrits par la famille.</summary>
    decimal? IEmulatorSettings.GetMemoryKib(MachineConfiguration configuration)
    {
        if ((configuration.Family == MachineFamily.St || configuration.Model == MachineModel.XlXe) && configuration.Options.TryGetValue(SettingsConstants.MainMemory, out var selected) && long.TryParse(selected, out var bytes)) return bytes / 1024m;
        return configuration.Family == MachineFamily.St ? StModelCatalog.Get(configuration.Model).DefaultMainMemoryKib : HardwareModelCatalog.Get(configuration.Model).MainMemoryBytes / 1024m;
    }


    /// <summary>Conserve la résolution des valeurs audio déjà utilisée par la famille.</summary>
    AudioConfiguration IEmulatorSettings.GetAudioConfiguration(MachineConfiguration configuration)
    {
        var output = configuration.Options.GetValueOrDefault(VideoAudioSettingsConstants.AudioOutputOption);
        if (string.Equals(output, VideoAudioSettingsConstants.DefaultAudioOutput, StringComparison.Ordinal)) output = null;
        var latency = configuration.Options.TryGetValue(VideoAudioSettingsConstants.AudioLatencyOption, out var configuredLatency) && int.TryParse(configuredLatency, out var parsedLatency) ? parsedLatency : AudioConstants.DefaultLatencyMilliseconds;
        return new(output, latency);
    }

    /// <summary>Conserve les choix de latence existants, remplacés par les contraintes du cœur lorsqu’il les redéfinit.</summary>
    IReadOnlyList<int> IEmulatorSettings.GetAudioLatencyChoices(MachineConfiguration configuration) => AudioConstants.LatencyChoicesMilliseconds;


    /// <summary>Conserve les options lorsque le cœur ne possède aucune conversion du choix graphique de l'hôte.</summary>
    /// <remarks>La présentation des images reste sous la responsabilité de l'hôte ; cette opération ne démarre aucun cœur et ne possède aucune surface graphique.</remarks>
    MachineConfiguration IEmulatorVideoSettings.ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi) => configuration;

    /// <summary>Reprend les champs de fichiers de firmware déjà décrits lorsque le backend ne fournit pas de catalogue explicite.</summary>
    /// <remarks>Le nom de destination et l'obligation ne sont pas déduits du libellé. Les champs d'information,
    /// les sélecteurs de modèles et les dossiers ne sont pas des emplacements de fichiers de firmware.</remarks>
    IReadOnlyList<FirmwareSlot> IEmulatorFirmwareAdapter.GetFirmwareSlots(MachineConfiguration configuration) => ((IEmulatorSettings)this).DescribeSettings(configuration)
        .SelectMany(block => block.Fields).Where(field => field.Editor == EmulationSettingsEditor.Path && (field.Tab == EmulationMachineTab.Rom || field.DefaultFolderCategory == EmulationDefaultFolderCategory.Firmware))
        .GroupBy(field => field.Id, StringComparer.Ordinal).Select(group => group.First())
        .Select(field => new FirmwareSlot(field.Id, string.Empty, field.LabelResourceKey, ExplanationResourceKey: field.ExplanationResourceKey, DetailedExplanationResourceKey: field.DetailedExplanationResourceKey)).ToArray();

    /// <summary>Prépare le dossier système quand aucun traitement natif de firmware ne le complète.</summary>
    void IEmulatorFirmwareAdapter.PrepareFirmware(MachineConfiguration configuration, string systemDirectory) => Directory.CreateDirectory(systemDirectory);

    /// <summary>Conserve la validation habituelle lorsque le cœur ne déclare aucune option différée.</summary>
    bool IEmulatorSettings.IsDeferredOption(MachineConfiguration configuration, string key) => false;

    /// <summary>Conserve la description du champ lorsque le cœur ne fournit aucune reconnaissance particulière de son horloge CPU.</summary>
    bool IEmulatorSettings.IsCpuClock(string fieldId) => false;

    /// <summary>Conserve le choix initial du module tant que le cœur ne déclare aucune recommandation.</summary>
    bool IEmulatorMachineDefaults.IsDefaultFor(string machineId) => false;

    /// <summary>Conserve la sélection séparée désactivée tant que le cœur ne la déclare pas.</summary>
    bool IEmulatorInitialContentSettings.SupportsInitialContent => false;

    /// <summary>Réutilise les médias configurés et conserve uniquement les sources insérées dans leur ordre de lecteurs.</summary>
    IReadOnlyList<string> IEmulatorInitialContentSettings.ResolveInitialContentPaths(MachineConfiguration configuration)
        => ResolveConfiguredMedia(configuration).Where(media => media.IsInserted).OrderBy(media => media.Slot).Select(media => media.Path).ToArray();

    /// <summary>Conserve l’absence de valeurs initiales propres au cœur ; les catalogues natifs peuvent fournir leur implémentation.</summary>
    IReadOnlyDictionary<string, string> IEmulatorConfigurationOptions.GetDefaultOptions(MachineConfiguration configuration) => new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Complète les valeurs manquantes sans écraser celles préparées par l’appelant.</summary>
    void IEmulatorConfigurationOptions.InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options)
    {
        foreach (var option in ((IEmulatorConfigurationOptions)this).GetDefaultOptions(configuration))
            options.TryAdd(option.Key, option.Value);
    }

    /// <summary>Réutilise le contrôle de configuration du cœur lorsque celui-ci ne sépare pas la validation des options.</summary>
    void IEmulatorConfigurationOptions.ValidateOptions(MachineConfiguration configuration) => ((IEmulatorConfigurationValidation)this).ValidateConfiguration(configuration);

    /// <summary>Conserve l’identité du modèle en l’absence de résolution d’une variante native.</summary>
    MachineModel IEmulatorConfigurationOptions.ResolveModel(MachineConfiguration configuration) => configuration.Model;

    /// <summary>Conserve les options empruntées lorsqu’aucune normalisation native n’est nécessaire.</summary>
    IReadOnlyDictionary<string, string> IEmulatorConfigurationOptions.NormalizeOptions(MachineConfiguration configuration) => configuration.Options;

    /// <summary>Réutilise les commandes décrites dans Common ; les cœurs émulant des touches conservent leur adaptation.</summary>
    IReadOnlyList<InputActionDefinition> IEmulatorControllerAdapter.DescribeInput(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => CommonControllerFunctions.InputProcessing.DescribeInput(InputSettingsFunctions.Describe(configuration), port, peripheral.ToString());

    /// <summary>Réutilise les définitions du choix existant sans recréer les libellés ni les directions opposées.</summary>
    IReadOnlyList<InputBindingDefinition>? IEmulatorControllerAdapter.GetControllerBindingDefinitions(MachineConfiguration configuration, PeripheralCategory peripheral, int port) => CommonControllerFunctions.InputProcessing.GetControllerBindingDefinitions(InputSettingsFunctions.Describe(configuration), peripheral.ToString(), port);

    /// <summary>Expose les ports matériels lorsque le cœur ne fournit aucune extension propre.</summary>
    int IEmulatorControllerAdapter.GetControllerPortCount(MachineConfiguration configuration) => CommonControllerFunctions.ModelPortCount(configuration);

    /// <summary>Conserve le nom déjà décrit pour le port ; un cœur peut fournir son organisation particulière.</summary>
    string? IEmulatorControllerAdapter.GetControllerPortDisplayName(MachineConfiguration configuration, int port) => CommonControllerFunctions.InputProcessing.GetControllerPort(InputSettingsFunctions.Describe(configuration), port)?.DisplayName;

    /// <summary>Conserve le numéro de présentation déjà décrit pour le port.</summary>
    int? IEmulatorControllerAdapter.GetControllerPortDisplayNumber(MachineConfiguration configuration, int port) => CommonControllerFunctions.InputProcessing.GetControllerPort(InputSettingsFunctions.Describe(configuration), port)?.DisplayNumber;

    /// <summary>Consulte les choix matériels existants lorsque le cœur ne fournit aucune liste propre.</summary>
    IReadOnlyList<PeripheralCategory> IEmulatorControllerAdapter.GetPeripherals(MachineConfiguration configuration) => CommonControllerFunctions.InputProcessing.GetPeripherals<PeripheralCategory>(InputSettingsFunctions.Describe(configuration));

    /// <summary>Consulte les choix existants du port demandé sans les mélanger avec ceux des autres ports.</summary>
    IReadOnlyList<PeripheralCategory> IEmulatorControllerAdapter.GetPeripherals(MachineConfiguration configuration, int port) => CommonControllerFunctions.InputProcessing.GetPeripherals<PeripheralCategory>(InputSettingsFunctions.Describe(configuration), port);

    /// <summary>Consulte les ports USB déjà décrits ; ne crée aucun bus ou périphérique absent du modèle.</summary>
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
    bool IEmulatorRuntimePolicy.RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path) => false;

    /// <summary>Ne demande par défaut aucun redémarrage supplémentaire pour une option.</summary>
    /// <remarks>Les métadonnées des options et les contraintes concrètes du backend restent appliquées par leur propriétaire.</remarks>
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
    void IEmulatorConfigurationValidation.ValidateConfiguration(MachineConfiguration configuration) { }

    /// <summary>Annonce par défaut l’absence d’entrées de pointage ; le backend déclare sa prise en charge effective.</summary>
    bool IEmulatorPointerCapabilities.SupportsPointer(MachineConfiguration configuration) => false;

    /// <summary>Réutilise la projection Common existante sans changer les associations ni les règles matérielles.</summary>
    EmulationInputSnapshot IEmulatorInputMapper.MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => InputSnapshotFunctions.Apply(snapshot, configuration.Input, configuration.Model);
    /// <summary>Réutilise la description Common existante pour le contrat commun de réglages.</summary>
    IReadOnlyList<EmulationSettingsBlock> IEmulatorSettings.DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context) => SettingsDescriptionFunctions.Create(configuration);
    IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration);
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
