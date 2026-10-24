using System.Globalization;
using GWGUI.Emulation.Example.Common.Services;

namespace GWGUI.Emulation.Example.Modules;

/// <summary>Façade publique du mock intégré ; chaque opération rejoint le Common du module.</summary>
/// <remarks>Les signatures restent identiques à celles d’un module indépendant. Aucun traitement de cœur,
/// algorithme SDK ou stockage n’est recopié dans cette façade.</remarks>
public sealed class ExampleEmulationModule : IEmulationModule, IEmulationEmulatorManager, IEmulationStorageSettingsManager,
    IEmulationInputSettingsManager, IEmulationFirmwareManager, IEmulationVideoSettingsManager, IEmulationModuleLocalization
{
    private readonly Engine _engine;
    /// <summary>Construit le Common avec les services empruntés de l’hôte, sans démarrer de machine.</summary>
    public ExampleEmulationModule(EmulationModuleContext context) => _engine = new(context);
    /// <inheritdoc/>
    public string Id => _engine.Id;
    /// <inheritdoc/>
    public string DisplayResourceKey => _engine.DisplayResourceKey;
    /// <inheritdoc/>
    public string BrandImageResourceName => _engine.BrandImageResourceName;
    /// <inheritdoc/>
    public IReadOnlyList<EmulationMachineDefinition> Machines => _engine.Machines;
    /// <inheritdoc/>
    public EmulationSettingsVisibility DefaultVisibility => _engine.DefaultVisibility;
    /// <summary>Transmet la commande au Common, qui indique si ce backend la reconnaît.</summary>
    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode) => _engine.TryHandleHostCommand(arguments, out exitCode);
    /// <summary>Résout les textes du module par Common et conserve les noms techniques invariants.</summary>
    public bool TryGetString(string key, CultureInfo culture, out string value) => _engine.TryGetString(key, culture, out value);
    /// <summary>Demande au Common la description des réglages du modèle sélectionné.</summary>
    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null) => _engine.Describe(machineId, configuration);
    /// <summary>Demande au Common une configuration neuve sans écrire de fichier.</summary>
    public IEmulationConfiguration CreateConfiguration(string machineId) => _engine.CreateConfiguration(machineId);
    /// <summary>Transmet le changement de modèle et conserve les règles de validation du Common.</summary>
    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId) => _engine.ChangeMachine(configuration, machineId);
    /// <summary>Applique les réglages par le Common ; la façade ne les interprète pas.</summary>
    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration, IReadOnlyDictionary<string, string?> values) => _engine.ApplySettings(configuration, values);
    /// <summary>Demande la projection des options appliquées par le module.</summary>
    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration) => _engine.RuntimeOptions(configuration);
    /// <summary>Demande les informations de la ligne de configuration.</summary>
    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) => _engine.SummarizeConfiguration(configuration);
    /// <summary>Prépare la fabrique de machine dans Common ; le runtime reste propriétaire de ses ressources créées.</summary>
    public ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default) => _engine.CreateRuntimeAsync(configuration, services, cancellationToken);
    /// <summary>Charge les configurations par le magasin Common.</summary>
    public ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => _engine.LoadConfigurationsAsync(cancellationToken);
    /// <summary>Enregistre par le magasin Common et son écriture atomique.</summary>
    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.SaveConfigurationAsync(configuration, cancellationToken);
    /// <summary>Supprime une configuration par son identité validée dans Common.</summary>
    public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default) => _engine.DeleteConfigurationAsync(configurationId, cancellationToken);
    /// <summary>Décrit le cœur intégré via Common.</summary>
    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default) => _engine.GetEmulatorInstallationAsync(machineId, cancellationToken);
    /// <summary>Décrit l’installation de l’adaptateur choisi dans la configuration.</summary>
    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.GetEmulatorInstallationAsync(configuration, cancellationToken);
    /// <summary>Présente les trois adaptateurs compatibles avec le profil courant.</summary>
    public ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.GetEmulatorInstallationsAsync(configuration, cancellationToken);
    /// <summary>Transmet au Common le choix explicite d’un émulateur.</summary>
    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default) => _engine.UseEmulatorAsync(configuration, emulatorId, cancellationToken);
    /// <summary>Demande les versions du cœur choisi, sans revenir au cœur préféré du modèle.</summary>
    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.FindEmulatorReleasesAsync(configuration, cancellationToken);
    /// <summary>Transmet la version au service du cœur effectivement choisi.</summary>
    public ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => _engine.InstallEmulatorAsync(configuration, release, progress, cancellationToken);
    /// <summary>Demande les versions proposées par Common ; le backend intégré n’en télécharge aucune.</summary>
    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default) => _engine.FindEmulatorReleasesAsync(machineId, cancellationToken);
    /// <summary>Transmet une demande d’installation au Common, seul responsable de sa politique.</summary>
    public ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => _engine.InstallEmulatorAsync(machineId, release, progress, cancellationToken);
    /// <summary>Décrit les médias et lecteurs par Common.</summary>
    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) => _engine.DescribeStorageSettings(configuration);
    /// <summary>Valide et applique les montages par Common.</summary>
    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings) => _engine.ApplyStorageSettings(configuration, settings);
    /// <summary>Décrit les associations mémorisées par Common.</summary>
    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => _engine.DescribeInputSettings(configuration);
    /// <summary>Transmet les associations au propriétaire de configuration Common.</summary>
    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => _engine.ApplyInputSettings(configuration, settings);
    /// <summary>Enregistre les associations par le magasin Common.</summary>
    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.SaveInputSettingsAsync(configuration, cancellationToken);
    /// <summary>Demande la racine de firmware au Common.</summary>
    public string GetFirmwareDirectory(string machineId) => _engine.GetFirmwareDirectory(machineId);
    /// <summary>Transmet la recherche de firmware au Common, sans scanner de dossier dans la façade.</summary>
    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.ScanFirmwareAsync(machineId, configuration, cancellationToken);
    /// <summary>Transmet le choix de firmware et sa validation au Common.</summary>
    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware) => _engine.UseFirmware(configuration, firmware);

    /// <summary>Transmet le choix graphique à Common par le contrat public du SDK.</summary>
    /// <param name="configuration">Configuration opaque de cette famille, conservée sous la responsabilité de son appelant.</param>
    /// <param name="graphicsApi">API choisie par l'hôte pour la présentation ou le rendu matériel.</param>
    /// <returns>Configuration adaptée par Common et par le cœur, sans créer de session ni de surface graphique.</returns>
    public IEmulationConfiguration ApplyVideoSettings(IEmulationConfiguration configuration, EmulationGraphicsApi graphicsApi) => _engine.ApplyVideoSettings(configuration, graphicsApi);
}
