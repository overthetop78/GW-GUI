namespace GWGUI.Emulation.Example.Common.Interfaces;
/// <summary>Contrat Common d’un backend : gestion de son installation, projection des médias et création de machines.</summary>
/// <remarks>Le contrat SDK hérité fournit les identifiants, la description, les commandes d’hôte et les opérations
/// de recherche et d’installation. Create reçoit les services empruntés et retourne une machine Common possédant
/// son cœur. La fabrique libère le cœur dans finally si sa construction échoue avant ce transfert de propriété.</remarks>
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorCapabilities, IEmulatorConfigurationSelection, IEmulatorConfigurationValidation,
    IEmulatorControllerAdapter, IEmulatorFirmwareAdapter, IEmulatorFirmwareCapabilities, IEmulatorFirmwareInspection, IEmulatorConfigurationOptions, IEmulatorContentFormats, IEmulatorControllerFeedbackAdapter,
    IEmulatorInitialContentSettings, IEmulatorPersistentStorage, IEmulatorRuntimePolicy, IEmulatorSettings, IEmulatorStorageSettings, IEmulatorVideoSettings
{
    /// <summary>Nom technique de la bibliothèque de cœur utilisée par l’adaptateur.</summary>
    /// <remarks>Ce membre conserve la métadonnée LibraryName des catalogues internes, sans dupliquer
    /// l’identité, le libellé et les modèles déjà portés par Definition. Un cœur intégré sans bibliothèque
    /// autonome fournit une chaîne vide ; cette description ne charge ni ne crée un fichier DLL.</remarks>
    string LibraryName { get; }

    /// <summary>Décrit les extensions de cartouche acceptées pour une configuration.</summary>
    /// <param name="configuration">Configuration empruntée précisant le modèle et ses options.</param>
    /// <returns>Ensemble des extensions spécifiques, ou null lorsque l’adaptateur n’en fournit pas de restriction spécifique.</returns>
    IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration);

    /// <summary>Décrit les emplacements de firmware proposés par cet adaptateur.</summary>
    /// <param name="configuration">Configuration du modèle pour lequel les emplacements sont demandés.</param>
    /// <returns>Descriptions des champs de sélection, fichiers attendus et contraintes associées.</returns>
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);

    /// <summary>Indique si le cœur exige au moins un fichier de firmware externe avant l’enregistrement de sa configuration.</summary>
    /// <remarks>Cette obligation peut être satisfaite par plusieurs alternatives facultatives prises séparément.
    /// IsRequired dans FirmwareSlot exprime, lui, l’obligation de fournir précisément cet emplacement.
    /// Un cœur disposant uniquement de firmwares embarqués annonce false.</remarks>
    bool RequiresExternalFirmware { get; }

    /// <summary>Recherche les candidats de firmware suivant les règles du modèle et du cœur.</summary>
    /// <param name="configuration">Configuration indiquant les emplacements et compatibilités à rechercher.</param>
    /// <param name="directory">Répertoire de recherche fourni par l’hôte, dont les fichiers restent empruntés.</param>
    /// <param name="cancellationToken">Annulation de la recherche et des lectures nécessaires à l’identification.</param>
    /// <returns>Candidats avec leurs identités et informations de compatibilité.</returns>
    IReadOnlyList<EmulationFirmwareCandidate> ScanFirmware(MachineConfiguration configuration, string directory, CancellationToken cancellationToken);

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

    /// <summary>Applique les champs de réglages propres au backend.</summary>
    /// <param name="configuration">Configuration de départ, conservée immuable.</param>
    /// <param name="values">Valeurs d’éditeurs indexées par identifiant de champ ; null représente une valeur non renseignée.</param>
    /// <returns>Configuration résultant des règles de ce backend.</returns>
    MachineConfiguration ApplyBackendSettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values);

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

    /// <summary>Extensions de contenus annoncées par l’adaptateur sans démarrer une machine.</summary>
    IReadOnlyList<string> SupportedContentExtensions { get; }

    /// <summary>Projette les réglages validés du module vers les valeurs textuelles attendues pendant l’exécution.</summary>
    /// <param name="configuration">Configuration de machine, conservée immuable par le module.</param>
    /// <returns>Associations de clés et valeurs techniques ; les nombres utilisent un format indépendant de la langue.</returns>
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);

    /// <summary>Prépare les fichiers de firmware dans le dossier système fourni au cœur.</summary>
    /// <param name="configuration">Configuration empruntée contenant les firmwares sélectionnés.</param>
    /// <param name="systemDirectory">Dossier de préparation dont Common possède le cycle de vie.</param>
    /// <remarks>Les fichiers sources de l’utilisateur restent empruntés et ne sont pas supprimés par cette opération.</remarks>
    void PrepareFirmware(MachineConfiguration configuration, string systemDirectory);

    /// <summary>Décrit les fichiers nécessaires au chargement d’un média, notamment les pistes référencées par un conteneur.</summary>
    /// <param name="path">Chemin du média principal.</param>
    /// <returns>Chemins des dépendances du média ; une liste vide indique qu’aucun fichier associé n’est nécessaire.</returns>
    IReadOnlyList<string> GetMediaDependencies(string path);
}
