namespace GWGUI.Emulation.Example.Common.Interfaces;
/// <summary>Contrat Common d’un backend : gestion de son installation, projection des médias et création de machines.</summary>
/// <remarks>Le contrat SDK hérité fournit les identifiants, la description, les commandes d’hôte et les opérations
/// de recherche et d’installation. Create reçoit les services empruntés et retourne une machine Common possédant
/// son cœur. La fabrique libère le cœur dans finally si sa construction échoue avant ce transfert de propriété.</remarks>
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorCapabilities, IEmulatorConfigurationSelection, IEmulatorConfigurationValidation,
    IEmulatorControllerAdapter, IEmulatorFirmwareAdapter, IEmulatorFirmwareSettings, IEmulatorFirmwareCapabilities, IEmulatorFirmwareInspection, IEmulatorConfigurationOptions, IEmulatorContentFormats, IEmulatorControllerFeedbackAdapter,
    IEmulatorInitialContentSettings, IEmulatorPersistentStorage, IEmulatorRuntimePolicy, IEmulatorSettingsAdapter, IEmulatorSettingsApplication, IEmulatorStorageSettings, IEmulatorVideoSettings, IEmulatorPointerCapabilities
{
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

    /// <summary>Nom technique de la bibliothèque de cœur utilisée par l’adaptateur.</summary>
    /// <remarks>Ce membre conserve la métadonnée LibraryName des catalogues internes, sans dupliquer
    /// l’identité, le libellé et les modèles déjà portés par Definition. Un cœur intégré sans bibliothèque
    /// autonome fournit une chaîne vide ; cette description ne charge ni ne crée un fichier DLL.</remarks>
    string LibraryName { get; }

    /// <summary>Décrit les extensions de cartouche acceptées pour une configuration.</summary>
    /// <param name="configuration">Configuration empruntée précisant le modèle et ses options.</param>
    /// <returns>Ensemble des extensions spécifiques, ou null lorsque l’adaptateur n’en fournit pas de restriction spécifique.</returns>
    IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration);

    /// <summary>Prépare un média pour les contraintes de contenu du cœur, avec conversion si nécessaire.</summary>
    /// <param name="media">Description du média source ; le fichier utilisateur reste sous la responsabilité de son propriétaire.</param>
    /// <param name="conversionDirectory">Répertoire fourni pour les contenus préparés.</param>
    /// <returns>Description du média utilisable par le cœur, éventuellement avec un chemin de contenu préparé.</returns>
    ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory);

    /// <summary>Prépare la configuration destinée à l’exécution du cœur.</summary>
    /// <param name="configuration">Configuration d’origine à adapter sans la modifier.</param>
    /// <param name="conversionDirectory">Répertoire de préparation des contenus nécessaire au runtime.</param>
    /// <returns>Configuration utilisable à l’exécution après les préparations propres au backend.</returns>
    Task<MachineConfiguration> PrepareRuntimeConfigurationAsync(MachineConfiguration configuration, string conversionDirectory);

    /// <summary>Interprète les indicateurs du cœur pour obtenir l’activité des lecteurs configurés.</summary>
    /// <param name="configuration">Configuration indiquant l’association entre lecteurs et médias.</param>
    /// <param name="ledStates">États des LED du cœur, empruntés pendant la projection.</param>
    /// <returns>Activité indexée par emplacement de média ; les LED ne correspondant pas à un lecteur ne deviennent pas de l’activité média.</returns>
    IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates);

    /// <summary>Produit le résumé de configuration propre au cœur sélectionné.</summary>
    /// <param name="configuration">Configuration à présenter sans démarrer son runtime.</param>
    /// <returns>Informations de modèle, processeur, mémoire, lecteurs et périphériques pour l’hôte.</returns>
    EmulationConfigurationSummary Summarize(MachineConfiguration configuration);

    /// <summary>Décrit les touches configurables propres au modèle et au cœur.</summary>
    /// <param name="configuration">Configuration déterminant le clavier et ses options.</param>
    /// <returns>Définitions de touches avec libellés et associations initiales.</returns>
    IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration);

    /// <summary>Adapte les entrées de configuration aux règles du backend.</summary>
    /// <param name="configuration">Configuration de départ avec les associations d’entrée choisies.</param>
    /// <returns>Configuration résultante après application des règles de périphériques du cœur.</returns>
    MachineConfiguration ApplyInputConfiguration(MachineConfiguration configuration);

    /// <summary>Combinaison de touches émulées utilisée par le backend pour sa réinitialisation logicielle.</summary>
    /// <remarks>Cette déclaration décrit les touches ; le propriétaire du runtime gère leur appui puis leur relâchement.</remarks>
    IReadOnlySet<EmulationKey> SoftResetKeys { get; }

    /// <summary>Projette les réglages validés du module vers les valeurs textuelles attendues pendant l’exécution.</summary>
    /// <param name="configuration">Configuration de machine, conservée immuable par le module.</param>
    /// <returns>Associations de clés et valeurs techniques ; les nombres utilisent un format indépendant de la langue.</returns>
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);

}
