using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using System.IO;
using GWGUI.Emulation.Sony.Modules;

namespace GWGUI.Emulation.Sony.Common.Services;

internal sealed class Engine
{
    /// <summary>Construit le module depuis le contexte fourni par l’hôte.</summary>
    /// <param name="context">Racine des données, dossier du module et client HTTP emprunté.</param>
    /// <returns>Façade utilisant les services Common, prête à décrire les machines sans démarrer de cœur.</returns>
    /// <remarks>La construction conserve les emplacements de configuration et de firmware ainsi que
    /// leur ordre de préparation. Le client HTTP reste sous la responsabilité de son propriétaire.
    /// La machine et les ressources du cœur sont créées séparément lors du lancement du runtime.</remarks>
    public static IEmulationModule Create(EmulationModuleContext context) {
        ArgumentNullException.ThrowIfNull(context);
        Directory.CreateDirectory(Path.Combine(context.ModuleDirectory, "Configurations"));
        return new SonyEmulationModule(Path.Combine(context.ModuleDirectory, "Configurations"), context.DataDirectory, context.HttpClient, Path.Combine(context.ModuleDirectory, "Core"));
    }

    private readonly IReadOnlyDictionary<string, IEmulatorAdapter> _adapters;

    public Engine()
    {
        var adapters = EmulatorCatalog.CreateAdapters();
        _adapters = adapters.ToDictionary(adapter => adapter.EmulatorId, StringComparer.Ordinal);
    }

    internal IEmulatedMachine CreateMachine(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(context);
        return Adapter(configuration).Create(configuration, context);
    }

    /// <summary>Résout le backend choisi dans le catalogue possédé par cette instance Common.</summary>
    /// <param name="configuration">Configuration empruntée portant la sélection du backend.</param>
    /// <returns>La même instance du catalogue, ou null lorsque la sélection ne correspond à aucun backend.</returns>
    internal IEmulatorAdapter? SelectedAdapter(MachineConfiguration configuration) => _adapters.Values.FirstOrDefault(item => item.EmulatorId == configuration.EmulatorId);

    internal IEmulatorAdapter Adapter(MachineConfiguration configuration) => Adapter(configuration.EmulatorId);

    internal IEmulatorAdapter Adapter(string emulatorId) => _adapters.TryGetValue(emulatorId, out var adapter) ? adapter : throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);

    internal bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        foreach (var adapter in _adapters.Values)
            if (adapter.TryHandleHostCommand(arguments, out exitCode)) return true;
        exitCode = 0;
        return false;
    }

    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(SonyEmulationModule).Assembly, "GWGUI.Emulation.Sony.Resources.Emulation");
    private readonly ConfigurationStore _store = null !;
    private readonly HttpClient _httpClient = null !;
    private readonly string _coreDirectory = null !;
    private readonly string _firmwareDirectory = null !;
    private readonly string _memoryStickDirectory = null !;
    private EmulatorManagementContext EmulatorManagement(IEmulatorAdapter adapter) => new(_httpClient, Path.Combine(_coreDirectory, adapter.EmulatorId));
    /// <summary>Configure les opérations du module avec ses chemins et son client HTTP emprunté ; la construction sans paramètres conserve le parcours de catalogue.</summary>
    public Engine(string configurationDirectory, string pathBase, HttpClient httpClient, string coreDirectory) : this()
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _httpClient = httpClient;
        _coreDirectory = coreDirectory;
        _memoryStickDirectory = Path.Combine(pathBase, "Emulation", "Storage", StorageSettingsFunctionsConstants.MemoryStickSubdirectory);
        _firmwareDirectory = Path.Combine(pathBase, EmulationPathConstants.RootDirectoryName, EmulationPathConstants.MachinesDirectoryName, EmulationModuleConstants.ModuleId, EmulationPathConstants.FirmwareDirectoryName);
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    public string BrandImageResourceName => $"{EmulationModuleConstants.AssetResourcePrefix}.sony.png";
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));

    public bool TryGetString(string key, System.Globalization.CultureInfo culture, out string value) => Localization.TryGetString(key, culture, out value);

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var current = configuration as MachineConfiguration ?? (MachineConfiguration)CreateConfiguration(machineId);
        var model = ModelCatalog.Get(current.Model);
        var hasAdapter = EmulatorCatalog.GetAll(model.Id).Any(adapter => adapter.Id == current.EmulatorId);
        var blocks = SettingsDescriptionFunctions.Create(current);
        var populatedTabs = blocks.Where(block => block.Fields.Count > 0).Select(block => block.Tab).ToHashSet();
        var tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
        {
            EmulationMachineTab.Rom => populatedTabs.Contains(EmulationMachineTab.Rom),
            EmulationMachineTab.Cheats or EmulationMachineTab.Ethernet => populatedTabs.Contains(item.Key),
            EmulationMachineTab.Keyboard => hasAdapter && (model.HasKeyboard || InputSettingsFunctions.HasUsbKeyboard(current)),
            EmulationMachineTab.Mouse => hasAdapter && (model.MouseButtonCount > 0 || InputSettingsFunctions.HasUsbMouse(current)),
            EmulationMachineTab.Controllers => hasAdapter,
            EmulationMachineTab.Storage => model.MaximumFloppyDriveCount > 0
                || model.SupportsCassetteDrive || model.SupportsCartridgeSlot
                || model.SupportsCompactDiscDrive,
            _ => item.Value
        });
        EmulationMachineSettings result = new EmulationMachineSettings(model.Id, new EmulationSettingsVisibility(tabs), blocks);
        if (SelectedAdapter(current)?.SupportsVideoSettings == false && !result.Blocks.Any(block => block.Tab == EmulationMachineTab.Video))
        {
            var adjustedTabs = result.Visibility.Tabs.ToDictionary(pair => pair.Key, pair => pair.Value);
            adjustedTabs[EmulationMachineTab.Video] = false;
            result = result with { Visibility = new EmulationSettingsVisibility(adjustedTabs) };
        }
        return result;
    }

    /// <summary>Consulte les recommandations compatibles avant de préparer une nouvelle configuration.</summary>
    /// <param name="configuration">Choix initial du module, déjà construit avec les données du modèle.</param>
    /// <returns>La configuration initiale sans recommandation différente, ou sa sélection adaptée par le cœur recommandé.</returns>
    /// <remarks>Un choix initial déjà recommandé reste prioritaire. Les autres recommandations sont départagées
    /// par identifiant ordinal, parmi les adaptateurs compatibles uniquement. Cette étape est réservée à la
    /// création initiale ; les changements explicites et les documents persistés conservent leur propre choix.</remarks>
    private MachineConfiguration SelectConfiguration(MachineConfiguration configuration)
    {
        var machineId = ((IEmulationConfiguration)configuration).MachineId;
        var current = SelectedAdapter(configuration);
        if (current?.IsDefaultFor(machineId) == true) return configuration;
        var preferred = _adapters.Values.Where(adapter => adapter.Definition.MachineIds.Contains(machineId))
            .OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).FirstOrDefault(adapter => adapter.IsDefaultFor(machineId));
        if (preferred is null || preferred.EmulatorId == current?.EmulatorId) return configuration;
        return preferred.SelectConfiguration(configuration with { EmulatorId = preferred.EmulatorId });
    }


    /// <summary>Prépare les valeurs initiales du cœur sélectionné par le contrat Common.</summary>
    /// <param name="configuration">Nouvelle configuration, entièrement construite et conservée immuable.</param>
    /// <returns>La même instance si les options restent identiques ; sinon une copie contenant les valeurs préparées.</returns>
    /// <remarks>Le dictionnaire transmis au cœur appartient à cette préparation. Les valeurs existantes,
    /// les données de la configuration et la distinction entre une collection absente et vide sont conservées
    /// lorsqu’aucun changement n’est apporté. Sans adaptateur sélectionné, la configuration reste inchangée.</remarks>
    private MachineConfiguration InitializeOptions(MachineConfiguration configuration)
    {
        var adapter = SelectedAdapter(configuration);
        if (adapter is null) return configuration;
        var original = configuration.Options;
        var options = original is null ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(original, StringComparer.Ordinal);
        adapter.InitializeOptions(configuration, options);
        if (original is null ? options.Count == 0 : options.Count == original.Count && options.All(item => original.TryGetValue(item.Key, out var value) && string.Equals(value, item.Value, StringComparison.Ordinal)))
            return configuration;
        return configuration with { Options = options };
    }


    public IEmulationConfiguration CreateConfiguration(string machineId)
    {
        var model = ModelCatalog.Get(machineId);
        return InitializeOptions(SelectConfiguration(new MachineConfiguration(model.Id, DefaultEmulatorId(model.Id),
            Options: model.Id == ModelConstants.Psp ? new Dictionary<string, string>(StringComparer.Ordinal)
            { [StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + 0] = _memoryStickDirectory }
                : new Dictionary<string, string>(StringComparer.Ordinal), Id: Guid.NewGuid(),
            Input: new InputConfiguration(), Media: [])));
    }

    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId)
    {
        if (configuration is not MachineConfiguration current)
            throw new ArgumentException(nameof(configuration));
        var created = (MachineConfiguration)CreateConfiguration(machineId);
        var emulatorId = EmulatorCatalog.GetAll(machineId).Any(item => item.Id == current.EmulatorId) ? current.EmulatorId : created.EmulatorId;
        return created with { Id = current.Id, EmulatorId = emulatorId,
            AudioEnabled = current.AudioEnabled, Audio = current.Audio, GraphicsApi = current.GraphicsApi };
    }

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var sony = RequireConfiguration(configuration);
        var adapter = SelectedAdapter(sony);
        var firmwareFields = (adapter is null ? [] : FirmwareConfigurationFunctions.Slots(sony, adapter)).Select(slot => slot.FieldId).ToHashSet(StringComparer.Ordinal);
        var options = new Dictionary<string, string>(sony.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        if (sony.Model == ModelConstants.Psp && string.IsNullOrWhiteSpace(options.GetValueOrDefault(StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + 0)))
            options[StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + 0] = _memoryStickDirectory;
        CommonCoreHostFunctions.OptionProcessing.ApplyValues(options, values,
            optionKey => firmwareFields.Contains(optionKey) || optionKey is SettingsConstants.Model or SettingsConstants.Emulator
                or SettingsConstants.AudioEnabled or SettingsConstants.AudioOutput
                or SettingsConstants.AudioLatency || optionKey.StartsWith(SettingsConstants.Model + ".", StringComparison.Ordinal));
        var audio = adapter?.GetAudioConfiguration(sony) ?? sony.Audio ?? new AudioConfiguration();
        var applied = adapter is { } ? adapter.ApplySettings(sony with { Options = options }, values) : sony with { Options = options };
        var updated = applied with
        {
            FirmwarePaths = adapter is null ? sony.FirmwarePaths : FirmwareConfigurationFunctions.Apply(sony, values, adapter).FirmwarePaths,
            AudioEnabled = values.TryGetValue(SettingsConstants.AudioEnabled, out var enabled) ? enabled == SettingsDescriptionFunctionsConstants.Enabled : sony.AudioEnabled,
            Audio = audio with
            {
                OutputDeviceId = values.TryGetValue(SettingsConstants.AudioOutput, out var output) ? string.IsNullOrWhiteSpace(output) ? null : output : audio.OutputDeviceId,
                LatencyMilliseconds = values.TryGetValue(SettingsConstants.AudioLatency, out var latency) && int.TryParse(latency, out var parsedLatency) ? parsedLatency : audio.LatencyMilliseconds
            }
        };
        if (adapter is null) return updated;
        updated = updated with { Audio = adapter.GetAudioConfiguration(updated) };
        return updated.GraphicsApi is { } api ? adapter.ApplyVideoSettings(updated, api) : updated;
    }

    public IEmulationConfiguration ApplyVideoSettings(IEmulationConfiguration configuration, EmulationGraphicsApi graphicsApi)
    {
        var sony = RequireConfiguration(configuration);
        var adapter = SelectedAdapter(sony);
        return adapter?.ApplyVideoSettings(sony, graphicsApi) ?? sony with { GraphicsApi = graphicsApi };
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration)
    {
        var sony = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var adapter = SelectedAdapter(sony);
        return adapter?.GetRuntimeOptions(sony) ?? sony.Options ?? new Dictionary<string, string>();
    }

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) => ConfigurationSummaryFunctions.Create(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)));

    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return SelectedAdapter(current)?.DescribeStorage(current) ?? StorageSettingsFunctions.Describe(current);
    }

    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return SelectedAdapter(current)?.ApplyStorage(current, settings) ?? StorageSettingsFunctions.Apply(current, settings);
    }

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => InputSettingsFunctions.Describe(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => InputSettingsFunctions.Apply(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is MachineConfiguration sony ? new ValueTask(_store.SaveAsync(sony, cancellationToken)) : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false)).Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration sony)
            return ValueTask.FromException(new ArgumentException(nameof(configuration)));
        ConfigurationValidationFunctions.ValidateForSave(sony);
        return new ValueTask(_store.SaveAsync(sony, cancellationToken));
    }

    public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var adapter = this.Adapter(DefaultEmulatorId(machineId));
        return adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken);
    }

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var adapter = this.Adapter(RequireConfiguration(configuration).EmulatorId);
        return adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var sony = RequireConfiguration(configuration);
        var installations = new List<EmulationEmulatorInstallation>();
        foreach (var emulator in EmulatorCatalog.GetAll(sony.Model))
        {
            var adapter = this.Adapter(emulator.Id);
            installations.Add(await adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken).ConfigureAwait(false));
        }
        return installations;
    }

    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sony = RequireConfiguration(configuration);
        if (!EmulatorCatalog.GetAll(sony.Model).Any(item => item.Id == emulatorId))
            return ValueTask.FromException<IEmulationConfiguration>(new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null));
        return ValueTask.FromResult<IEmulationConfiguration>(this.Adapter(emulatorId).SelectConfiguration(sony with { EmulatorId = emulatorId }));
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var adapter = this.Adapter(DefaultEmulatorId(machineId));
        return adapter.FindReleasesAsync(EmulatorManagement(adapter), cancellationToken);
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var adapter = this.Adapter(RequireConfiguration(configuration).EmulatorId);
        return adapter.FindReleasesAsync(EmulatorManagement(adapter), cancellationToken);
    }

    public ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var adapter = this.Adapter(DefaultEmulatorId(machineId));
        return adapter.InstallAsync(EmulatorManagement(adapter), release, progress, cancellationToken);
    }

    public ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var adapter = this.Adapter(RequireConfiguration(configuration).EmulatorId);
        return adapter.InstallAsync(EmulatorManagement(adapter), release, progress, cancellationToken);
    }

    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration sony)
            throw new ArgumentException(nameof(configuration));
        var adapter = this.Adapter(sony);
        var corePath = await adapter.FindInstalledCorePathAsync(EmulatorManagement(adapter), cancellationToken).ConfigureAwait(false) ?? throw CommonCoreHostFunctions.Messages.EmulatorNotInstalled(adapter.EmulatorId);
        var audio = adapter.GetAudioConfiguration(sony);
        sony = sony with { Audio = audio };
        var context = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
            value => Path.Combine(services.StatesDirectory, value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat), CoreDirectoryConstants.SavesDirectoryName));
        var storage = StorageSettingsFunctions.Describe(sony);
        var mounted = adapter.ResolveConfiguredMedia(sony).Concat(storage.AvailableDevices
            .Where(device => device.ConfigurationKind == EmulationStorageConfigurationKind.MemoryCard && storage.ConfiguredSlots.Contains(device.Slot))
            .Select(device => (device, path: storage.DeviceSettings?.FirstOrDefault(item => item.Slot == device.Slot)?.MemoryCardPath))
            .Where(item => !string.IsNullOrWhiteSpace(item.path))
            .Select(item => new EmulationMedia(item.path!, item.device.Slot, EmulationMediaType.MemoryCard, false, true)))
            .DistinctBy(media => media.Slot).ToArray();
        return new EmulationMachineRuntime(sony,
            media => this.CreateMachine(EmulationMediaConversionFunctions.WithMedia(sony, media), context),
            storage.AvailableDevices.Where(device => storage.ConfiguredSlots.Contains(device.Slot)).ToArray(), mounted,
            MachineConfigurationConstants.ResourceKey(sony.Model),
            SupportsPointerCapture: adapter.SupportsPointerCapture(sony));
    }

    public string GetFirmwareDirectory(string machineId)
    {
        _ = ModelCatalog.Get(machineId);
        return _firmwareDirectory;
    }

    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var current = RequireConfiguration(configuration);
        if (current.Model != machineId) throw new ArgumentException(nameof(machineId));
        var directory = GetFirmwareDirectory(machineId);
        if (!Directory.Exists(directory)) return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>([]);
        var adapter = this.Adapter(current);
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>(Directory.EnumerateFiles(directory, FirmwareCatalogConstants.SearchPattern, SearchOption.AllDirectories).SelectMany(path => adapter.ScanFirmwareFileAsync(current, path, cancellationToken).GetAwaiter().GetResult()).ToArray());
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware)
    {
        var current = RequireConfiguration(configuration);
        var adapter = this.Adapter(current);
        if (!FirmwareConfigurationFunctions.Slots(current, adapter).Any(slot => slot.FieldId == firmware.DestinationFieldId))
            throw new ArgumentException(nameof(firmware));
        if (!File.Exists(firmware.Path)) throw new FileNotFoundException(null, firmware.Path);
        return FirmwareConfigurationFunctions.Apply(current, new Dictionary<string, string?> { [firmware.DestinationFieldId!] = Path.GetFullPath(firmware.Path) }, adapter);
    }

    private static string DefaultEmulatorId(string machineId) => EmulatorCatalog.GetAll(machineId).FirstOrDefault()?.Id ?? string.Empty;

    private static MachineConfiguration RequireConfiguration(IEmulationConfiguration configuration) => configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
}
