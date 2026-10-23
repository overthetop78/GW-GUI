using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Amstrad.Common.Functions.CoreHostFunctions;
using System.IO;
using GWGUI.Emulation.Amstrad.Modules;

namespace GWGUI.Emulation.Amstrad.Common.Services;

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
        Directory.CreateDirectory(Path.Combine(context.ModuleDirectory, EmulationPathConstants.FirmwareDirectoryName));
        return new AmstradEmulationModule(
            Path.Combine(context.ModuleDirectory, ConfigurationStoreConstants.ConfigurationsDirectoryName),
            context.DataDirectory, context.HttpClient,
            Path.Combine(context.ModuleDirectory, EmulationModuleConstants.CoreDirectoryName));
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

    internal IEmulatorAdapter Adapter(MachineConfiguration configuration) => Adapter(configuration.EmulatorId);


    internal IEmulatorAdapter Adapter(string emulatorId) => _adapters.TryGetValue(emulatorId, out var adapter) ? adapter : throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);

    internal bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        foreach (var adapter in _adapters.Values)
            if (adapter.TryHandleHostCommand(arguments, out exitCode)) return true;
        exitCode = EmulationModuleConstants.DefaultHostExitCode;
        return false;
    }

    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(AmstradEmulationModule).Assembly, CommonExceptionsConstants.ResourceManagerBaseName);
    private readonly ConfigurationStore _store = null !;
    private readonly HttpClient _httpClient = null !;
    private readonly string _coreDirectory = null !;
    private EmulatorManagementContext EmulatorManagement(IEmulatorAdapter adapter) => new(_httpClient, Path.Combine(_coreDirectory, adapter.EmulatorId));
    /// <summary>Configure les opérations du module avec ses chemins et son client HTTP emprunté ; la construction sans paramètres conserve le parcours de catalogue.</summary>
    public Engine(string configurationDirectory, string pathBase, HttpClient httpClient, string coreDirectory) : this()
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _httpClient = httpClient;
        _coreDirectory = coreDirectory;
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    public string BrandImageResourceName => $"{EmulationModuleConstants.AssetResourcePrefix}{EmulationModuleConstants.ModuleIconResourceSuffix}";
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));

    public bool TryGetString(string key, System.Globalization.CultureInfo culture, out string value) => Localization.TryGetString(key, culture, out value);

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var current = configuration as MachineConfiguration ?? (MachineConfiguration)CreateConfiguration(machineId);
        var model = ModelCatalog.Get(current.Model);
        var blocks = SettingsDescriptionFunctions.Create(current, EmulatorManagement(this.Adapter(current)));
        var capabilities = this.Adapter(current) as IEmulatorCapabilities;
        var tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
        {
            EmulationMachineTab.Video => capabilities?.SupportsVideoSettings != false || blocks.Any(block => block.Tab == EmulationMachineTab.Video),
            EmulationMachineTab.Keyboard => model.HasKeyboard,
            EmulationMachineTab.Mouse => capabilities?.SupportsPointer(current) != false && model.MouseButtonCount > BufferConstants.EmptyCollectionCount,
            EmulationMachineTab.Storage => model.MaximumFloppyDriveCount > BufferConstants.EmptyCollectionCount
                || model.SupportsCassetteDrive || model.SupportsCartridgeSlot,
            _ => item.Value
        });
        return new EmulationMachineSettings(model.Id, new EmulationSettingsVisibility(tabs), blocks);
    }

    public IEmulationConfiguration CreateConfiguration(string machineId)
    {
        var model = ModelCatalog.Get(machineId);
        return new MachineConfiguration(model.Id, DefaultEmulatorId(model.Id),
            Options: new Dictionary<string, string>(StringComparer.Ordinal), Id: Guid.NewGuid(),
            Controllers: Enumerable.Repeat(ControllerCatalog.Default(model), model.ControllerPortCount).ToArray(),
            Input: new InputConfiguration(), Media: []);
    }

    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId)
    {
        if (configuration is not MachineConfiguration current)
            throw new ArgumentException(nameof(configuration));
        var created = (MachineConfiguration)CreateConfiguration(machineId);
        var emulatorId = EmulatorCatalog.GetAll(machineId).Any(item => item.Id == current.EmulatorId) ? current.EmulatorId : created.EmulatorId;
        return created with { Id = current.Id, EmulatorId = emulatorId,
            AudioEnabled = current.AudioEnabled, Audio = current.Audio };
    }

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var amstrad = RequireConfiguration(configuration);
        var options = new Dictionary<string, string>(amstrad.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        CommonCoreHostFunctions.OptionProcessing.ApplyValues(options, values,
            optionKey => optionKey is SettingsConstants.Model or SettingsConstants.Emulator
                or SettingsConstants.AudioEnabled or SettingsConstants.AudioOutput
                or SettingsConstants.AudioLatency || optionKey.StartsWith(SettingsConstants.Model + ConfigurationStoreConstants.TemporaryNameSeparator, StringComparison.Ordinal));
        var audio = amstrad.Audio ?? new AudioConfiguration();
        var result = amstrad with
        {
            Options = options,
            AudioEnabled = values.TryGetValue(SettingsConstants.AudioEnabled, out var enabled) ? enabled == SettingsDescriptionFunctionsConstants.Enabled : amstrad.AudioEnabled,
            Audio = audio with
            {
                OutputDeviceId = values.TryGetValue(SettingsConstants.AudioOutput, out var output) ? string.IsNullOrWhiteSpace(output) ? null : output : audio.OutputDeviceId,
                LatencyMilliseconds = values.TryGetValue(SettingsConstants.AudioLatency, out var latency) && int.TryParse(latency, out var parsedLatency) ? parsedLatency : audio.LatencyMilliseconds
            }
        };
        result = SelectedAdapter(result)?.ApplySettings(result, values) ?? result;
        return result;
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var adapter = string.IsNullOrEmpty(current.EmulatorId) ? null : _adapters.GetValueOrDefault(current.EmulatorId);
        return adapter?.GetRuntimeOptions(current) ?? current.Options ?? new Dictionary<string, string>();
    }

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) => ConfigurationSummaryFunctions.Create(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)));

    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) => StorageSettingsFunctions.Describe(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings) => StorageSettingsFunctions.Apply(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => DescribeInput(RequireConfiguration(configuration));

    private EmulationInputSettings DescribeInput(MachineConfiguration configuration)
    {
        var settings = InputSettingsFunctions.Describe(configuration);
        return this.Adapter(configuration) is IEmulatorPointerCapabilities capabilities && !capabilities.SupportsPointer(configuration) ? settings with { Mouse = null } : settings;
    }

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => InputSettingsFunctions.Apply(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is MachineConfiguration amstrad ? new ValueTask(_store.SaveAsync(amstrad, cancellationToken)) : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false)).Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration amstrad)
            return ValueTask.FromException(new ArgumentException(nameof(configuration)));
        ConfigurationValidationFunctions.ValidateForSave(amstrad);
        return new ValueTask(_store.SaveAsync(amstrad, cancellationToken));
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
        var amstrad = RequireConfiguration(configuration);
        var installations = new List<EmulationEmulatorInstallation>();
        foreach (var emulator in EmulatorCatalog.GetAll(amstrad.Model))
        {
            var adapter = this.Adapter(emulator.Id);
            installations.Add(await adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken).ConfigureAwait(false));
        }
        return installations;
    }

    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var amstrad = RequireConfiguration(configuration);
        if (!EmulatorCatalog.GetAll(amstrad.Model).Any(item => item.Id == emulatorId))
            return ValueTask.FromException<IEmulationConfiguration>(new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null));
        var selected = amstrad with { EmulatorId = emulatorId };
        if (this.Adapter(selected) is IEmulatorConfigurationSelection selection) selected = selection.SelectConfiguration(selected);
        return ValueTask.FromResult<IEmulationConfiguration>(selected);
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
        if (configuration is not MachineConfiguration amstrad)
            throw new ArgumentException(nameof(configuration));
        var adapter = this.Adapter(amstrad);
        var corePath = await adapter.FindInstalledCorePathAsync(EmulatorManagement(adapter), cancellationToken).ConfigureAwait(false) ?? throw CommonCoreHostFunctions.Messages.EmulatorNotInstalled(adapter.EmulatorId);
        var audio = amstrad.Audio ?? new AudioConfiguration();
        var context = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
            value => Path.Combine(services.StatesDirectory, value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat), CoreDirectoryConstants.SavesDirectoryName));
        var storage = StorageSettingsFunctions.Describe(amstrad);
        var mounted = adapter.ResolveConfiguredMedia(amstrad);
        return new EmulationMachineRuntime(amstrad,
            media => this.CreateMachine(WithMedia(amstrad, media), context),
            storage.AvailableDevices.Where(device => storage.ConfiguredSlots.Contains(device.Slot)).ToArray(), mounted,
            MachineConfigurationConstants.ResourcePrefix + amstrad.Model,
            SupportsPointerCapture: adapter.SupportsPointerCapture(amstrad));
    }

    private static MachineConfiguration WithMedia(MachineConfiguration configuration, IEnumerable<EmulationMedia> media) => configuration with
    {
        Media = media.Select((item, index) => new MediaConfiguration(item.Path,
            item.Type switch
            {
                EmulationMediaType.Floppy => MediaCategory.Floppy,
                EmulationMediaType.Cassette => MediaCategory.Cassette,
                EmulationMediaType.Cartridge => MediaCategory.Cartridge,
                _ => throw new ArgumentOutOfRangeException(nameof(media), item.Type, null)
            }, IsReadOnly: item.IsReadOnly, IsInserted: item.IsInserted,
            MountOrder: index)).ToArray()
    };

    private static string DefaultEmulatorId(string machineId) => EmulatorCatalog.GetAll(machineId).FirstOrDefault()?.Id ?? string.Empty;

    private static MachineConfiguration RequireConfiguration(IEmulationConfiguration configuration) => configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
    /// <summary>Résout l’adaptateur sélectionné sans imposer un cœur aux configurations encore incomplètes.</summary>
    /// <param name="configuration">Configuration empruntée portant la sélection enregistrée.</param>
    /// <returns>Adaptateur du catalogue existant, ou null sans sélection correspondante.</returns>
    internal IEmulatorAdapter? SelectedAdapter(MachineConfiguration configuration) => _adapters.Values.FirstOrDefault(candidate => candidate.EmulatorId == configuration.EmulatorId);
}
