using System.Globalization;
using GWGUI.Emulation.Example.Common.Services;

namespace GWGUI.Emulation.Example.Modules;

/// <summary>Façade publique du mock intégré ; chaque opération rejoint le Common du module.</summary>
/// <remarks>Les signatures restent identiques à celles d’un module indépendant. Aucun traitement de cœur,
/// algorithme SDK ou stockage n’est recopié dans cette façade.</remarks>
public sealed class ExampleEmulationModule : IEmulationModule, IEmulationEmulatorManager, IEmulationStorageSettingsManager,
    IEmulationInputSettingsManager, IEmulationFirmwareManager, IEmulationVideoSettingsManager, IEmulationModuleLocalization
{
    /// <summary>Conserve le service Common auquel la façade publique confie ses opérations.</summary>
    /// <remarks><para>Le moteur organise les configurations, les catalogues et les fabriques. La façade ne duplique pas leurs traitements et ne possède pas les services empruntés à l&apos;application.</para></remarks>
    private readonly Engine _engine;
    /// <summary>Construit le Common avec les services empruntés de l’hôte, sans démarrer de machine.</summary>
    /// <param name="context">Contexte du module, avec ses dossiers et son client HTTP emprunté ; ce dernier reste possédé par l&apos;application.</param>
    public ExampleEmulationModule(EmulationModuleContext context) => _engine = new(context);
    /// <summary>Expose l&apos;identifiant stable du module fourni par Common.</summary>
    /// <remarks><para>L&apos;application utilise cette identité indépendamment du libellé traduit de la famille.</para></remarks>
    public string Id => _engine.Id;
    /// <summary>Expose la clé de ressource du nom de famille fourni par Common.</summary>
    /// <remarks><para>L&apos;application résout la traduction ; la façade conserve la clé technique au lieu de recopier les textes dans chaque langue.</para></remarks>
    public string DisplayResourceKey => _engine.DisplayResourceKey;
    /// <summary>Expose le nom de la ressource de marque annoncé par Common.</summary>
    /// <remarks><para>Le consommateur charge la ressource de présentation ; consulter son nom ne crée pas de surface graphique.</para></remarks>
    public string BrandImagePath => _engine.BrandImagePath;
    /// <summary>Expose les deux profils matériels décrits par le catalogue Common.</summary>
    /// <remarks><para>Les descriptions servent à la sélection et à la configuration. Elles ne sont pas des sessions d&apos;émulation et n&apos;initialisent aucun cœur.</para></remarks>
    public IReadOnlyList<EmulationMachineDefinition> Machines => _engine.Machines;
    /// <summary>Expose la visibilité initiale des onglets de réglages annoncée par Common.</summary>
    /// <remarks><para>Ces indications de présentation sont distinctes des capacités d&apos;exécution du cœur et des valeurs enregistrées dans une configuration.</para></remarks>
    public EmulationSettingsVisibility DefaultVisibility => _engine.DefaultVisibility;
    /// <summary>Transmet la commande au Common, qui indique si ce backend la reconnaît.</summary>
    /// <param name="arguments">Arguments de la commande d&apos;hôte, empruntés dans leur ordre d&apos;origine.</param>
    /// <param name="exitCode">Code de résultat de la commande reconnue, ou valeur de repli lorsque celle-ci n&apos;est pas traitée.</param>
    /// <returns>True si un adaptateur a reconnu et traité la commande ; false pour laisser l&apos;hôte poursuivre son parcours.</returns>
    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode) => _engine.TryHandleHostCommand(arguments, out exitCode);
    /// <summary>Résout les textes du module par Common et conserve les noms techniques invariants.</summary>
    /// <param name="key">Clé de ressource à résoudre par la localisation du module.</param>
    /// <param name="culture">Culture demandée pour la résolution des ressources, avant le repli géré par le service de localisation.</param>
    /// <param name="value">Texte résolu en sortie lorsque la clé est disponible, selon le repli du service de localisation.</param>
    /// <returns>True lorsque le service de localisation a résolu la clé pour la culture demandée ou son repli.</returns>
    public bool TryGetString(string key, CultureInfo culture, out string value) => _engine.TryGetString(key, culture, out value);
    /// <summary>Demande au Common la description des réglages du modèle sélectionné.</summary>
    /// <param name="machineId">Identifiant stable du profil matériel, résolu et validé par le catalogue Common.</param>
    /// <param name="configuration">Configuration empruntée à décrire, ou null pour consulter les réglages initiaux du modèle ou du catalogue.</param>
    /// <returns>Description publique des blocs de réglages et de leur visibilité pour le modèle demandé.</returns>
    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null) => _engine.Describe(machineId, configuration);
    /// <summary>Demande au Common une configuration neuve sans écrire de fichier.</summary>
    /// <param name="machineId">Identifiant stable du profil matériel, résolu et validé par le catalogue Common.</param>
    /// <returns>Nouvelle configuration initialisée pour le modèle demandé, avec les choix et options de son adaptateur.</returns>
    public IEmulationConfiguration CreateConfiguration(string machineId) => _engine.CreateConfiguration(machineId);
    /// <summary>Transmet le changement de modèle et conserve les règles de validation du Common.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="machineId">Identifiant stable du profil matériel, résolu et validé par le catalogue Common.</param>
    /// <returns>Configuration du profil demandé conservant les données compatibles et l&apos;identité de la configuration.</returns>
    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId) => _engine.ChangeMachine(configuration, machineId);
    /// <summary>Applique les réglages par le Common ; la façade ne les interprète pas.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="values">Valeurs proposées par les éditeurs, indexées par identifiant de champ ; la collection reste empruntée.</param>
    /// <returns>Configuration résultant des valeurs de champs proposées, sans modification des collections de la source.</returns>
    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration, IReadOnlyDictionary<string, string?> values) => _engine.ApplySettings(configuration, values);
    /// <summary>Demande la projection des options appliquées par le module.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Options du runtime annoncées par l&apos;adaptateur sélectionné pour la configuration.</returns>
    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration) => _engine.RuntimeOptions(configuration);
    /// <summary>Demande les informations de la ligne de configuration.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Résumé public des données matérielles et de l&apos;adaptateur de la configuration.</returns>
    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) => _engine.SummarizeConfiguration(configuration);
    /// <summary>Prépare la fabrique de machine dans Common ; le runtime reste propriétaire de ses ressources créées.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="services">Services de runtime empruntés ; les ressources créées par leurs fabriques sont gérées par leurs propriétaires de session.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente du runtime préparé ; celui-ci fournit la fabrique de machines et conserve les services nécessaires à leur exécution.</returns>
    public ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default) => _engine.CreateRuntimeAsync(configuration, services, cancellationToken);
    /// <summary>Charge les configurations par le magasin Common.</summary>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente de la liste des configurations chargées et validées par le magasin Common.</returns>
    public ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => _engine.LoadConfigurationsAsync(cancellationToken);
    /// <summary>Enregistre par le magasin Common et son écriture atomique.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente de l&apos;enregistrement de la configuration par le magasin Common.</returns>
    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.SaveConfigurationAsync(configuration, cancellationToken);
    /// <summary>Supprime une configuration par son identité validée dans Common.</summary>
    /// <param name="configurationId">Identité stable de la configuration à supprimer du magasin.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente de la suppression de la configuration enregistrée.</returns>
    public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default) => _engine.DeleteConfigurationAsync(configurationId, cancellationToken);
    /// <summary>Décrit le cœur intégré via Common.</summary>
    /// <param name="machineId">Identifiant stable du profil matériel, résolu et validé par le catalogue Common.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente de la description de l&apos;installation du cœur choisi ou recommandé pour cette demande.</returns>
    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default) => _engine.GetEmulatorInstallationAsync(machineId, cancellationToken);
    /// <summary>Décrit l’installation de l’adaptateur choisi dans la configuration.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente de la description de l&apos;installation du cœur choisi ou recommandé pour cette demande.</returns>
    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.GetEmulatorInstallationAsync(configuration, cancellationToken);
    /// <summary>Présente les trois adaptateurs compatibles avec le profil courant.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente des descriptions d&apos;installation des adaptateurs compatibles avec la configuration.</returns>
    public ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.GetEmulatorInstallationsAsync(configuration, cancellationToken);
    /// <summary>Transmet au Common le choix explicite d’un émulateur.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="emulatorId">Identifiant stable de l&apos;adaptateur demandé, distinct de son nom affiché.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente de la configuration retournée après sélection et préparation de l&apos;adaptateur demandé.</returns>
    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default) => _engine.UseEmulatorAsync(configuration, emulatorId, cancellationToken);
    /// <summary>Demande les versions du cœur choisi, sans revenir au cœur préféré du modèle.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente de la liste des versions proposées par l&apos;adaptateur retenu pour cette demande.</returns>
    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.FindEmulatorReleasesAsync(configuration, cancellationToken);
    /// <summary>Transmet la version au service du cœur effectivement choisi.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="release">Description de la version demandée, interprétée par le service Common de l&apos;adaptateur.</param>
    /// <param name="progress">Rapporteur facultatif de progression de l&apos;installation, transmis au service de l&apos;adaptateur.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente du chemin de l&apos;implémentation retenue par l&apos;adaptateur après traitement de la version demandée.</returns>
    public ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => _engine.InstallEmulatorAsync(configuration, release, progress, cancellationToken);
    /// <summary>Demande les versions proposées par Common ; le backend intégré n’en télécharge aucune.</summary>
    /// <param name="machineId">Identifiant stable du profil matériel, résolu et validé par le catalogue Common.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente de la liste des versions proposées par l&apos;adaptateur retenu pour cette demande.</returns>
    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default) => _engine.FindEmulatorReleasesAsync(machineId, cancellationToken);
    /// <summary>Transmet une demande d’installation au Common, seul responsable de sa politique.</summary>
    /// <param name="machineId">Identifiant stable du profil matériel, résolu et validé par le catalogue Common.</param>
    /// <param name="release">Description de la version demandée, interprétée par le service Common de l&apos;adaptateur.</param>
    /// <param name="progress">Rapporteur facultatif de progression de l&apos;installation, transmis au service de l&apos;adaptateur.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente du chemin de l&apos;implémentation retenue par l&apos;adaptateur après traitement de la version demandée.</returns>
    public ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => _engine.InstallEmulatorAsync(machineId, release, progress, cancellationToken);
    /// <summary>Décrit les médias et lecteurs par Common.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Description publique des lecteurs et des montages de la configuration.</returns>
    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) => _engine.DescribeStorageSettings(configuration);
    /// <summary>Valide et applique les montages par Common.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="settings">Réglages publics proposés par l&apos;hôte et consultés sans modifier ses collections.</param>
    /// <returns>Configuration résultant de l&apos;application des réglages de stockage par Common.</returns>
    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings) => _engine.ApplyStorageSettings(configuration, settings);
    /// <summary>Décrit les associations mémorisées par Common.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <returns>Description publique des associations d&apos;entrées actuellement configurées.</returns>
    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => _engine.DescribeInputSettings(configuration);
    /// <summary>Transmet les associations au propriétaire de configuration Common.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="settings">Réglages publics proposés par l&apos;hôte et consultés sans modifier ses collections.</param>
    /// <returns>Configuration résultant des associations d&apos;entrées proposées, après les traitements Common.</returns>
    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => _engine.ApplyInputSettings(configuration, settings);
    /// <summary>Enregistre les associations par le magasin Common.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente de l&apos;enregistrement des associations d&apos;entrées de la configuration.</returns>
    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.SaveInputSettingsAsync(configuration, cancellationToken);
    /// <summary>Demande la racine de firmware au Common.</summary>
    /// <param name="machineId">Identifiant stable du profil matériel, résolu et validé par le catalogue Common.</param>
    /// <returns>Chemin du dossier de firmware géré par Common pour ce modèle.</returns>
    public string GetFirmwareDirectory(string machineId) => _engine.GetFirmwareDirectory(machineId);
    /// <summary>Transmet la recherche de firmware au Common, sans scanner de dossier dans la façade.</summary>
    /// <param name="machineId">Identifiant stable du profil matériel, résolu et validé par le catalogue Common.</param>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="cancellationToken">Annulation transmise aux opérations Common ; le propriétaire conserve la responsabilité du nettoyage des ressources.</param>
    /// <returns>Attente de la liste des fichiers candidats et de leurs destinations compatibles, sans installation dans une session.</returns>
    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.ScanFirmwareAsync(machineId, configuration, cancellationToken);
    /// <summary>Transmet le choix de firmware et sa validation au Common.</summary>
    /// <param name="configuration">Configuration complète empruntée, contenant le modèle, l&apos;adaptateur, les options, les entrées et les montages.</param>
    /// <param name="firmware">Candidat de firmware retenu, avec son chemin et son champ de destination proposés.</param>
    /// <returns>Configuration portant le firmware retenu après contrôle de sa destination et de sa compatibilité.</returns>
    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware) => _engine.UseFirmware(configuration, firmware);

    /// <summary>Transmet le choix graphique à Common par le contrat public du SDK.</summary>
    /// <param name="configuration">Configuration opaque de cette famille, conservée sous la responsabilité de son appelant.</param>
    /// <param name="graphicsApi">API choisie par l'hôte pour la présentation ou le rendu matériel.</param>
    /// <returns>Configuration adaptée par Common et par le cœur, sans créer de session ni de surface graphique.</returns>
    public IEmulationConfiguration ApplyVideoSettings(IEmulationConfiguration configuration, EmulationGraphicsApi graphicsApi) => _engine.ApplyVideoSettings(configuration, graphicsApi);
}
