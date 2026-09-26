using System.IO;

namespace GWGUI.Emulation.Amstrad.Modules;

public sealed class AmstradEmulationModule : IEmulationModule, IEmulationEmulatorManager,
    IEmulationInputSettingsManager, IEmulationStorageSettingsManager, IEmulationModuleLocalization
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(AmstradEmulationModule).Assembly, "GWGUI.Emulation.Amstrad.Resources.Emulation");
    private readonly ConfigurationStore _store;
    private readonly HttpClient _httpClient;
    private readonly string _coreDirectory;
    private readonly Engine _engine = new();
    private EmulatorManagementContext EmulatorManagement(IEmulatorAdapter adapter) =>
        new(_httpClient, Path.Combine(_coreDirectory, adapter.EmulatorId));

    public AmstradEmulationModule(string configurationDirectory, string pathBase,
        HttpClient httpClient, string coreDirectory)
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _httpClient = httpClient;
        _coreDirectory = coreDirectory;
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(
        Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));

    public bool TryGetString(string key, System.Globalization.CultureInfo culture, out string value) =>
        Localization.TryGetString(key, culture, out value);

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode) =>
        _engine.TryHandleHostCommand(arguments, out exitCode);

    public EmulationMachineSettings Describe(string machineId,
        IEmulationConfiguration? configuration = null)
    {
        var current = configuration as MachineConfiguration
            ?? (MachineConfiguration)CreateConfiguration(machineId);
        var model = ModelCatalog.Get(current.Model);
        var tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
        {
            EmulationMachineTab.Keyboard => model.HasKeyboard,
            EmulationMachineTab.Mouse => model.MouseButtonCount > 0,
            EmulationMachineTab.Storage => model.MaximumFloppyDriveCount > 0
                || model.SupportsCassetteDrive || model.SupportsCartridgeSlot,
            _ => item.Value
        });
        return new EmulationMachineSettings(model.Id, new EmulationSettingsVisibility(tabs),
            SettingsDescriptionFunctions.Create(current));
    }

    public IEmulationConfiguration CreateConfiguration(string machineId)
    {
        var model = ModelCatalog.Get(machineId);
        return new MachineConfiguration(model.Id, DefaultEmulatorId(model.Id),
            Options: new Dictionary<string, string>(StringComparer.Ordinal), Id: Guid.NewGuid(),
            Controllers: Enumerable.Repeat(ControllerType.Joystick,
                model.ControllerPortCount).ToArray(),
            Input: new InputConfiguration(), Media: []);
    }

    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration,
        string machineId)
    {
        if (configuration is not MachineConfiguration current)
            throw new ArgumentException(nameof(configuration));
        var created = (MachineConfiguration)CreateConfiguration(machineId);
        var emulatorId = EmulatorCatalog.GetAll(machineId)
            .Any(item => item.Id == current.EmulatorId)
            ? current.EmulatorId
            : created.EmulatorId;
        return created with { Id = current.Id, EmulatorId = emulatorId,
            AudioEnabled = current.AudioEnabled, Audio = current.Audio };
    }

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration,
        IReadOnlyDictionary<string, string?> values)
    {
        var amstrad = RequireConfiguration(configuration);
        var options = new Dictionary<string, string>(amstrad.Options
            ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var item in values)
        {
            if (item.Key is SettingsConstants.Model or SettingsConstants.Emulator
                or SettingsConstants.AudioEnabled or SettingsConstants.AudioOutput
                or SettingsConstants.AudioLatency || item.Key.StartsWith(
                    SettingsConstants.Model + ".", StringComparison.Ordinal)) continue;
            if (item.Value is null) options.Remove(item.Key);
            else options[item.Key] = item.Value;
        }
        var audio = amstrad.Audio ?? new AudioConfiguration();
        return amstrad with
        {
            Options = options,
            AudioEnabled = values.TryGetValue(SettingsConstants.AudioEnabled, out var enabled)
                ? enabled == SettingsDescriptionFunctionsConstants.Enabled : amstrad.AudioEnabled,
            Audio = audio with
            {
                OutputDeviceId = values.TryGetValue(SettingsConstants.AudioOutput, out var output)
                    ? string.IsNullOrWhiteSpace(output) ? null : output : audio.OutputDeviceId,
                LatencyMilliseconds = values.TryGetValue(SettingsConstants.AudioLatency, out var latency)
                    && int.TryParse(latency, out var parsedLatency)
                    ? parsedLatency : audio.LatencyMilliseconds
            }
        };
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration) =>
        configuration is MachineConfiguration amstrad
            ? amstrad.Options ?? new Dictionary<string, string>()
            : throw new ArgumentException(nameof(configuration));

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) =>
        ConfigurationSummaryFunctions.Create(configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) =>
        StorageSettingsFunctions.Describe(configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration,
        EmulationStorageSettings settings) => StorageSettingsFunctions.Apply(
        configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)),
        settings);

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) =>
        InputSettingsFunctions.Describe(configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration,
        EmulationInputSettings settings) => InputSettingsFunctions.Apply(
        configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)),
        settings);

    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is MachineConfiguration amstrad
        ? new ValueTask(_store.SaveAsync(amstrad, cancellationToken))
        : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(
        CancellationToken cancellationToken = default) =>
        (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false))
        .Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration amstrad)
            return ValueTask.FromException(new ArgumentException(nameof(configuration)));
        ConfigurationValidationFunctions.ValidateForSave(amstrad);
        return new ValueTask(_store.SaveAsync(amstrad, cancellationToken));
    }

    public ValueTask DeleteConfigurationAsync(Guid configurationId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId,
        CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var adapter = _engine.Adapter(DefaultEmulatorId(machineId));
        return adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken);
    }

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var adapter = _engine.Adapter(RequireConfiguration(configuration).EmulatorId);
        return adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var amstrad = RequireConfiguration(configuration);
        var installations = new List<EmulationEmulatorInstallation>();
        foreach (var emulator in EmulatorCatalog.GetAll(amstrad.Model))
        {
            var adapter = _engine.Adapter(emulator.Id);
            installations.Add(await adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken)
                .ConfigureAwait(false));
        }
        return installations;
    }

    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration,
        string emulatorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var amstrad = RequireConfiguration(configuration);
        if (!EmulatorCatalog.GetAll(amstrad.Model).Any(item => item.Id == emulatorId))
            return ValueTask.FromException<IEmulationConfiguration>(
                new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null));
        return ValueTask.FromResult<IEmulationConfiguration>(amstrad with { EmulatorId = emulatorId });
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(
        string machineId, CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var adapter = _engine.Adapter(DefaultEmulatorId(machineId));
        return adapter.FindReleasesAsync(EmulatorManagement(adapter), cancellationToken);
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var adapter = _engine.Adapter(RequireConfiguration(configuration).EmulatorId);
        return adapter.FindReleasesAsync(EmulatorManagement(adapter), cancellationToken);
    }

    public ValueTask<string> InstallEmulatorAsync(string machineId,
        EmulationEmulatorRelease release, IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var adapter = _engine.Adapter(DefaultEmulatorId(machineId));
        return adapter.InstallAsync(EmulatorManagement(adapter), release, progress, cancellationToken);
    }

    public ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration,
        EmulationEmulatorRelease release, IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var adapter = _engine.Adapter(RequireConfiguration(configuration).EmulatorId);
        return adapter.InstallAsync(EmulatorManagement(adapter), release, progress, cancellationToken);
    }

    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(
        IEmulationConfiguration configuration, EmulationRuntimeServices services,
        CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration amstrad)
            throw new ArgumentException(nameof(configuration));
        var adapter = _engine.Adapter(amstrad);
        var corePath = await adapter.FindInstalledCorePathAsync(EmulatorManagement(adapter), cancellationToken)
            .ConfigureAwait(false) ?? throw new EmulationMessageException(new EmulationMessage(
                EmulationMessageCategory.Emulator, EmulationMessageCode.EmulatorNotInstalled,
                EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog,
                new EmulationEmulatorMessageContext(adapter.EmulatorId)));
        var audio = amstrad.Audio ?? new AudioConfiguration();
        var context = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
            value => Path.Combine(services.StatesDirectory,
                value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat),
                CoreDirectoryConstants.SavesDirectoryName));
        var storage = StorageSettingsFunctions.Describe(amstrad);
        var mounted = adapter.ResolveConfiguredMedia(amstrad);
        return new EmulationMachineRuntime(amstrad,
            media => _engine.CreateMachine(WithMedia(amstrad, media), context),
            storage.AvailableDevices.Where(device => storage.ConfiguredSlots.Contains(device.Slot)).ToArray(), mounted,
            MachineConfigurationConstants.ResourcePrefix + amstrad.Model,
            SupportsPointerCapture: ModelCatalog.Get(amstrad.Model).MouseButtonCount > 0);
    }

    private static MachineConfiguration WithMedia(MachineConfiguration configuration,
        IEnumerable<EmulationMedia> media) => configuration with
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

    private static string DefaultEmulatorId(string machineId) =>
        EmulatorCatalog.GetAll(machineId).FirstOrDefault()?.Id
        ?? throw new ArgumentOutOfRangeException(nameof(machineId), machineId, null);

    private static MachineConfiguration RequireConfiguration(IEmulationConfiguration configuration) =>
        configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
}
