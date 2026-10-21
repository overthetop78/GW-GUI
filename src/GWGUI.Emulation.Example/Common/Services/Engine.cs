using System.Globalization;
using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Example.Common.Constants;
using GWGUI.Emulation.Example.Common.Dictionaries;
using GWGUI.Emulation.Example.Common.Enums;
using GWGUI.Emulation.Example.Common.Exceptions;
using GWGUI.Emulation.Example.Common.Interfaces;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;
using GWGUI.Emulation.Example.Common.Machines.Common.Functions;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Résout les adaptateurs du module et leur confie la création des machines et les commandes d’hôte.</summary>
/// <remarks>Le catalogue associe les identifiants aux adaptateurs. La création d’une machine reste une opération de
/// l’adaptateur sélectionné ; ce service ne lance pas lui-même la boucle d’émulation.</remarks>
public sealed class Engine
{
    /// <summary>Chemin de l’assembly qui contient les trois simulations, sans bibliothèque externe à créer.</summary>
    internal static string CoreImplementationPath => typeof(Engine).Assembly.Location;
    /// <summary>Version d’implémentation commune aux trois cœurs livrés dans ce même assembly.</summary>
    internal static string CoreVersion => typeof(Engine).Assembly.GetName().Version!.ToString(3);
    /// <summary>Empreinte calculée une seule fois par l’interface publique d’inspection des cœurs.</summary>
    private static readonly Lazy<string> ImplementationHash = new(() => EmulationSdk.CoreDiagnostics.CalculateSha256(CoreImplementationPath));
    /// <summary>Identité binaire conservée dans les en-têtes d’état, complétée par le modèle et l’émulateur.</summary>
    internal static string CoreSha256 => ImplementationHash.Value;
    /// <summary>Adaptateurs du catalogue indexés par identifiant exact, avec comparaison ordinale.</summary>
    private readonly IReadOnlyDictionary<string, IEmulatorAdapter> _adapters;
    /// <summary>Magasin de configurations fourni seulement aux instances utilisées par la façade publique.</summary>
    private readonly ConfigurationStore? _store;
    /// <summary>Services de gestion empruntés ; le moteur ne détruit jamais le client HTTP de l’application.</summary>
    private readonly EmulatorManagementContext? _managementContext;
    /// <summary>Racine des firmwares utilisateur, fournie par le contexte du module.</summary>
    private readonly string? _firmwareDirectory;

    /// <summary>Construit le moteur de la façade à partir des mêmes services qu’un module indépendant.</summary>
    /// <remarks>Cette construction ne lance aucune machine et n’ouvre aucun contenu. Les autres instances
    /// sans contexte servent aux descriptions et validations pures, sans fabriquer de services de l’application.</remarks>
    public Engine(EmulationModuleContext context) : this()
    {
        ArgumentNullException.ThrowIfNull(context);
        _store = new ConfigurationStore(context.ModuleDirectory, context.DataDirectory);
        _managementContext = new EmulatorManagementContext(context.HttpClient, Path.Combine(context.ModuleDirectory, CoreDirectoryConstants.CoreDirectoryName));
        _firmwareDirectory = Path.Combine(context.ModuleDirectory, CoreDirectoryConstants.SystemDirectoryName);
    }

    /// <summary>Identité technique partagée par la façade et ses configurations.</summary>
    public string Id => EmulationModuleConstants.ModuleId;
    /// <summary>Clé du nom invariant du module.</summary>
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    /// <summary>Aucune image de marque n’est nécessaire pour utiliser les interfaces de cet exemple.</summary>
    public string BrandImageResourceName => string.Empty;
    /// <summary>Deux modèles fictifs décrits par le catalogue Common.</summary>
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    /// <summary>Onglets disponibles, dont la visibilité détaillée dépend ensuite des capacités déclarées.</summary>
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));
    /// <summary>Résout uniquement les noms techniques invariants ; les textes communs restent localisés par l’hôte.</summary>
    public bool TryGetString(string key, CultureInfo culture, out string value)
    {
        value = key == DisplayResourceKey ? "Example" : HardwareModelCatalog.All.FirstOrDefault(model => model.DisplayNameResourceKey == key)?.Model.ToString() ?? string.Empty;
        return value.Length != 0;
    }
    /// <summary>Décrit les réglages du modèle et les options de l’émulateur effectivement choisi.</summary>
    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var current = configuration is null ? new MachineConfiguration(ModelCatalog.Parse(machineId)) : Require(configuration);
        if (current.MachineId != machineId) throw new ArgumentException(nameof(machineId));
        return new(machineId, DefaultVisibility, SettingsDescriptionFunctions.Create(current, GetOptions(current, Management)));
    }
    /// <summary>Crée une configuration neuve sans fichier ni ressource d’exécution.</summary>
    public IEmulationConfiguration CreateConfiguration(string machineId) => new MachineConfiguration(ModelCatalog.Parse(machineId));
    /// <summary>Change le profil en conservant l’identité et les données compatibles avec ses lecteurs.</summary>
    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId)
    {
        var current = Require(configuration);
        var model = ModelCatalog.Parse(machineId);
        var hardware = HardwareModelCatalog.Get(model);
        var result = current with { Model = model, Media = current.Media.Where(media => hardware.Media.Contains(media.Category)).ToArray(), Firmwares = current.Firmwares.Where(firmware => hardware.Firmware.Contains(firmware.Category)).ToArray() };
        ConfigurationFunctions.Validate(result);
        return result;
    }
    /// <summary>Applique et valide les champs par leur propriétaire Common.</summary>
    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration, IReadOnlyDictionary<string, string?> values) => SettingsApplicationFunctions.Apply(Require(configuration), values);
    /// <summary>Demande les options de runtime à l’adaptateur choisi, sans sélectionner un autre émulateur.</summary>
    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration) => Adapter(Require(configuration)).GetRuntimeOptions(Require(configuration));
    /// <summary>Produit le résumé de la configuration par le catalogue matériel Common.</summary>
    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) => ConfigurationSummaryFunctions.Create(Require(configuration));
    /// <summary>Décrit les lecteurs du profil et les montages actuellement enregistrés.</summary>
    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) => StorageSettingsFunctions.Describe(Require(configuration));
    /// <summary>Applique les montages sans ouvrir ni déplacer les fichiers de contenu.</summary>
    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings) => StorageSettingsFunctions.Apply(Require(configuration), settings);
    /// <summary>Décrit les catégories génériques d’entrées et leurs associations.</summary>
    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => InputSettingsFunctions.Describe(Require(configuration));
    /// <summary>Valide et applique les associations par leur contrat Common.</summary>
    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => InputSettingsFunctions.Apply(Require(configuration), settings);
    /// <summary>Persiste les associations au sein du même document que les autres réglages.</summary>
    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => SaveConfigurationAsync(configuration, cancellationToken);
    /// <summary>Charge les documents par le magasin Common, avec propagation de l’annulation.</summary>
    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => (await Store.LoadAllAsync(cancellationToken).ConfigureAwait(false)).Cast<IEmulationConfiguration>().ToArray();
    /// <summary>Enregistre une configuration validée par l’écriture atomique du magasin.</summary>
    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => new(Store.SaveAsync(Require(configuration), cancellationToken));
    /// <summary>Supprime uniquement les données de la configuration possédées par le magasin.</summary>
    public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default) { cancellationToken.ThrowIfCancellationRequested(); Store.Delete(configurationId); return ValueTask.CompletedTask; }
    /// <summary>Refuse les opérations persistantes sur un moteur purement descriptif sans contexte d’hôte.</summary>
    private ConfigurationStore Store => _store ?? throw new InvalidOperationException(nameof(EmulationModuleContext));
    /// <summary>Refuse de fabriquer implicitement un client HTTP ou une racine de stockage manquante.</summary>
    private EmulatorManagementContext Management => _managementContext ?? throw new InvalidOperationException(nameof(EmulationModuleContext));
    /// <summary>Vérifie l’origine et la cohérence du document reçu par une interface publique.</summary>
    private static MachineConfiguration Require(IEmulationConfiguration configuration)
    {
        if (configuration is not MachineConfiguration current) throw new ArgumentException(nameof(configuration));
        ConfigurationFunctions.Validate(current);
        return current;
    }

    /// <summary>Résout l’adaptateur préféré uniquement pour les appels qui ne possèdent pas encore de configuration.</summary>
    private IEmulatorAdapter PreferredAdapter(string machineId) => Adapter(DefaultEmulator(ModelCatalog.Parse(machineId)));
    /// <summary>Décrit la version intégrée de l’adaptateur préféré du profil.</summary>
    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default) => PreferredAdapter(machineId).GetInstallationAsync(Management, cancellationToken);
    /// <summary>Décrit la version de l’émulateur effectivement choisi dans le document.</summary>
    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => Adapter(Require(configuration)).GetInstallationAsync(Management, cancellationToken);
    /// <summary>Présente les trois adaptateurs compatibles afin que l’utilisateur puisse choisir chacun d’eux.</summary>
    public async ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var results = new List<EmulationEmulatorInstallation>();
        foreach (var adapter in Adapters(Require(configuration).MachineId)) results.Add(await adapter.GetInstallationAsync(Management, cancellationToken).ConfigureAwait(false));
        return results;
    }
    /// <summary>Enregistre le choix explicite d’un adaptateur compatible, sans recréer de configuration.</summary>
    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current = Require(configuration);
        var adapter = Adapter(emulatorId);
        if (!adapter.Definition.MachineIds.Contains(current.MachineId)) throw new ArgumentException(nameof(emulatorId));
        var result = current with { Core = Enum.Parse<Emulator>(adapter.EmulatorKey) };
        ConfigurationFunctions.Validate(result);
        return ValueTask.FromResult<IEmulationConfiguration>(result);
    }
    /// <summary>Présente la version locale du cœur préféré lorsqu’aucun document n’est fourni.</summary>
    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default) => PreferredAdapter(machineId).FindReleasesAsync(Management, cancellationToken);
    /// <summary>Présente la version locale du cœur choisi dans la configuration.</summary>
    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => Adapter(Require(configuration)).FindReleasesAsync(Management, cancellationToken);
    /// <summary>Valide la version fournie par un adaptateur compatible avec ce profil, sans téléchargement.</summary>
    public ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => PreferredAdapter(machineId).InstallAsync(Management, release, progress, cancellationToken);
    /// <summary>Transmet l’installation au cœur effectivement choisi.</summary>
    public ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => Adapter(Require(configuration)).InstallAsync(Management, release, progress, cancellationToken);

    /// <summary>Prépare la fabrique sérialisée du runtime, sans démarrer la boucle avant la demande de l’hôte.</summary>
    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default)
    {
        var current = Require(configuration);
        var adapter = Adapter(current);
        var path = await adapter.FindInstalledCorePathAsync(Management, cancellationToken).ConfigureAwait(false) ?? throw new FileNotFoundException();
        current = await adapter.PrepareRuntimeConfigurationAsync(current, services.ConvertedMediaDirectory).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var audio = adapter.GetAudioConfiguration(current);
        var context = new EmulatorCreationContext(services.SessionsDirectory, path, services.HostExecutablePath, () => services.CreateAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds), value => Store.MemoryDirectory(value.Id));
        var storage = StorageSettingsFunctions.Describe(current);
        return new(current, media => CreateMachine(current with { Media = media.Select(item => EmulationMediaConversionFunctions.ToModule(item, current.Media)).ToArray() }, context), storage.AvailableDevices, storage.MountedMedia, HardwareModelCatalog.Get(current.Model).DisplayNameResourceKey, InputSettingsFunctions.SupportsPointerCapture(current.Input), (media, token) => { token.ThrowIfCancellationRequested(); return adapter.PrepareMediaAsync(media, services.ConvertedMediaDirectory); });
    }
    /// <summary>Résout la racine des firmwares après validation du modèle ; ne la crée pas implicitement.</summary>
    public string GetFirmwareDirectory(string machineId) { _ = ModelCatalog.Parse(machineId); return _firmwareDirectory ?? throw new InvalidOperationException(nameof(EmulationModuleContext)); }
    /// <summary>Recherche les fichiers compatibles suivant le contrat d’inspection du cœur choisi.</summary>
    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var current = Require(configuration);
        if (current.MachineId != machineId) throw new ArgumentException(nameof(machineId));
        return ValueTask.FromResult(Adapter(current).ScanFirmware(current, GetFirmwareDirectory(machineId), cancellationToken));
    }
    /// <summary>Applique le firmware facultatif uniquement après validation de son rôle et de son fichier.</summary>
    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware)
    {
        var current = Require(configuration);
        if (!Enum.TryParse<FirmwareCategory>(firmware.DestinationFieldId, out var category) || !Adapter(current).IsFirmwareCompatible(current, category, firmware.Path)) throw new ArgumentException(nameof(firmware));
        var result = current with { Firmwares = current.Firmwares.Where(item => item.Category != category).Append(new FirmwareConfiguration(category, firmware.Path, false)).ToArray() };
        ConfigurationFunctions.Validate(result);
        return result;
    }
    /// <summary>Transmet le choix de présentation au contrat vidéo de l’adaptateur.</summary>
    public IEmulationConfiguration ApplyVideoSettings(IEmulationConfiguration configuration, EmulationGraphicsApi graphicsApi) => Adapter(Require(configuration)).ApplyVideoSettings(Require(configuration), graphicsApi);

    /// <summary>Construit les adaptateurs du catalogue et vérifie implicitement l’unicité de leurs identifiants.</summary>
    /// <exception cref="ArgumentException">Plusieurs adaptateurs portent le même identifiant.</exception>
    public Engine()
    {
        var adapters = EmulatorCatalog.CreateAdapters();
        _adapters = adapters.ToDictionary(adapter => adapter.EmulatorId, StringComparer.Ordinal);
    }

    /// <summary>Valide la configuration puis demande une machine à l’adaptateur sélectionné.</summary>
    /// <param name="configuration">Configuration du modèle, du cœur et des contenus.</param>
    /// <param name="context">Services et répertoires de création transmis à l’adaptateur.</param>
    /// <returns>Machine créée, dont l’appelant doit gérer le cycle de vie et la libération asynchrone.</returns>
    /// <exception cref="ArgumentNullException">La configuration ou le contexte est null.</exception>
    /// <exception cref="EmulationException">Le catalogue ne propose aucun runtime pour ce modèle.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Aucun adaptateur ne correspond au cœur choisi.</exception>
    internal IEmulatedMachine CreateMachine(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(context);
        ConfigurationFunctions.Validate(configuration);
        if (!ConfigurationFunctions.SupportsRuntime(configuration.Model))
            throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreNotFound, ErrorMessages.CoreFileMissing);
        return Adapter(configuration).Create(configuration, context);
    }

    /// <summary>Résout l’adaptateur du cœur enregistré dans une configuration.</summary>
    /// <param name="configuration">Configuration contenant le choix du cœur.</param>
    /// <returns>Adaptateur conservé dans ce moteur.</returns>
    /// <exception cref="KeyNotFoundException">L’identifiant du catalogue n’est pas présent parmi les adaptateurs.</exception>
    internal IEmulatorAdapter Adapter(MachineConfiguration configuration) => Adapter(configuration.Core);

    /// <summary>Résout le backend choisi lorsqu’un choix effectif existe.</summary>
    /// <param name="configuration">Configuration pouvant encore porter le choix None.</param>
    /// <returns>Adaptateur sélectionné, ou null sans sélection ; aucune sélection implicite n’est enregistrée.</returns>
    /// <remarks>Les écrans de configuration peuvent décrire le modèle avant la sélection d’un cœur.
    /// La création du runtime utilise Adapter et sa validation obligatoire.</remarks>
    internal IEmulatorAdapter? SelectedAdapter(MachineConfiguration configuration) => configuration.Core == Emulator.None ? null : Adapter(configuration);

    /// <summary>Consulte la préférence du catalogue pour un modèle.</summary>
    /// <param name="model">Modèle dont les adaptateurs compatibles sont recherchés.</param>
    /// <returns>Choix explicite du catalogue, premier compatible ou None en l’absence de backend.</returns>
    /// <remarks>Le résultat décrit une préférence ; cette consultation ne modifie pas une configuration existante.</remarks>
    internal Emulator DefaultEmulator(MachineModel model) => EmulatorCatalog.DefaultEmulator(model);

    /// <summary>Demande les options du backend sélectionné par son contrat de réglages.</summary>
    /// <param name="configuration">Configuration permettant de résoudre les options dépendantes du modèle.</param>
    /// <param name="context">Services de gestion empruntés lorsque le backend doit consulter son installation.</param>
    /// <returns>Descriptions courantes, ou une liste vide lorsque le backend n’expose pas de réglages.</returns>
    /// <remarks>Le backend reçoit le contexte sans en prendre possession. Ce moteur ne charge et ne conserve aucun cœur.</remarks>
    internal IReadOnlyList<CoreOption> GetOptions(MachineConfiguration configuration, EmulatorManagementContext context) =>
        SelectedAdapter(configuration) is IEmulatorSettings settings ? settings.GetSettingsOptions(configuration, context) : [];
    /// <summary>Résout un choix de cœur typé en passant par son identifiant de catalogue.</summary>
    /// <param name="emulator">Valeur du cœur à rechercher.</param>
    /// <returns>Adaptateur associé à son identifiant.</returns>
    internal IEmulatorAdapter Adapter(Emulator emulator) => Adapter(EmulatorCatalog.Get(emulator).Id);

    /// <summary>Recherche un adaptateur par son identifiant exact.</summary>
    /// <param name="emulatorId">Identifiant invariant du cœur.</param>
    /// <returns>Adaptateur conservé dans le moteur.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Aucun adaptateur n’a cet identifiant.</exception>
    internal IEmulatorAdapter Adapter(string emulatorId) => _adapters.TryGetValue(emulatorId, out var adapter) ? adapter : throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);

    /// <summary>Sélectionne les adaptateurs déclarant prendre en charge un modèle.</summary>
    /// <param name="machineId">Identifiant de modèle déclaré dans les définitions d’adaptateurs.</param>
    /// <returns>Nouvelle liste triée par identifiant d’adaptateur avec comparaison ordinale.</returns>
    internal IReadOnlyList<IEmulatorAdapter> Adapters(string machineId) => _adapters.Values.Where(adapter => adapter.Definition.MachineIds.Contains(machineId)).OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).ToArray();

    /// <summary>Présente la ligne de commande à chaque adaptateur jusqu’à ce que l’un la reconnaisse.</summary>
    /// <param name="arguments">Arguments du processus hôte, empruntés et transmis aux adaptateurs.</param>
    /// <param name="exitCode">Code fourni par l’adaptateur qui traite la commande, sinon code de succès par défaut.</param>
    /// <returns>Vrai si un adaptateur a traité la commande ; faux sinon.</returns>
    internal bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        foreach (var adapter in _adapters.Values)
            if (adapter.TryHandleHostCommand(arguments, out exitCode)) return true;
        exitCode = CoreHostConstants.NativeOperationSuccess;
        return false;
    }
}
