namespace GWGUI.Emulation.Philips.Common.Interfaces;
internal interface IEmulatorAdapter : IEmulatorControllerFeedbackAdapter, IEmulatorCapabilities, IEmulatorContentFormats, IEmulatorPersistentStorage, IEmulatorConfigurationSelection, IEmulatorFirmwareAdapter, GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorRuntimePolicy, IEmulatorInputMapper, IEmulatorMediaCapabilities, IEmulatorConfigurationValidation, IEmulatorSettings, IEmulatorPointerCapabilities, IEmulatorVideoSettings, IEmulatorSettingsApplication, IEmulatorStorageSettings
{
    /// <summary>Ne déclare aucune sortie de contrôleur lorsque le backend ne décrit pas de retour physique.</summary>
    /// <param name="configuration">Configuration empruntée du modèle et des ports.</param>
    /// <param name="peripheral">Catégorie du périphérique émulé à décrire.</param>
    /// <returns>Liste vide ; les implémentations concrètes fournissent les commandes et libellés de leurs effets.</returns>
    /// <remarks>Cette description ne démarre aucun effet et ne possède aucun périphérique physique.</remarks>
    IReadOnlyList<EmulationControllerOutputDefinition> IEmulatorControllerFeedbackAdapter.GetControllerOutputs(MachineConfiguration configuration, PeripheralCategory peripheral) => [];

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

    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
