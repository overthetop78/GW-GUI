using GWGUI.Emulation;
using System.Globalization;
using System.IO;
using System.Net.Http;

namespace GWGUI.Emulation.Fairchild.Modules;

public sealed class FairchildEmulationModule : IEmulationModule, IEmulationEmulatorManager,
    IEmulationInputSettingsManager, IEmulationStorageSettingsManager, IEmulationModuleLocalization,
    IEmulationFirmwareManager
{
    private static readonly EmulationModuleLocalization Localization = new(typeof(FairchildEmulationModule).Assembly, EmulationModuleConstants.LocalizationResourceName);
    private readonly ConfigurationStore _store;
    private readonly EmulatorManagementContext _managementContext;
    private readonly Engine _engine = new();
    private readonly string _firmwareDirectory;

    public FairchildEmulationModule(string configurationDirectory, string pathBase, HttpClient httpClient, string coreDirectory)
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _managementContext = new EmulatorManagementContext(httpClient, coreDirectory);
        _firmwareDirectory = Path.Combine(pathBase, EmulationPathConstants.RootDirectoryName, EmulationPathConstants.MachinesDirectoryName, EmulationModuleConstants.ModuleId, EmulationPathConstants.FirmwareDirectoryName);
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    // Brand image is independent of the machine photographs.
    public string BrandImageResourceName => $"{EmulationModuleConstants.AssetResourcePrefix}.fairchild.png";
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, tab => true));

    public bool TryGetString(string key, CultureInfo culture, out string value) => Localization.TryGetString(key, culture, out value);
    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode) => _engine.TryHandleHostCommand(arguments, out exitCode);

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var model = ModelCatalog.Parse(machineId);
        var current = configuration is null ? new MachineConfiguration(model) : Require(configuration);
        if (current.Model != model) throw new ArgumentException(nameof(configuration));
        var tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
        {
            EmulationMachineTab.Keyboard => false,
            EmulationMachineTab.Mouse => false,
            EmulationMachineTab.Rom => FirmwareFunctions.Roles(current.Model).Count > 0,
            EmulationMachineTab.Cpu => true,
            EmulationMachineTab.Ram => HardwareModelCatalog.Get(current.Model).MainMemoryBytes is not null,
            EmulationMachineTab.Controllers => ControllerConstants.PortCount
                > BufferConstants.EmptyCollectionCount,
            EmulationMachineTab.Storage => true,
            _ => item.Value
        });
        return new EmulationMachineSettings(machineId, new EmulationSettingsVisibility(tabs), SettingsDescriptionFunctions.Create(current));
    }

    public IEmulationConfiguration CreateConfiguration(string machineId) => new MachineConfiguration(ModelCatalog.Parse(machineId));
    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId)
    {
        _ = Require(configuration);
        return CreateConfiguration(machineId);
    }

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var current = Require(configuration);
        var editable = GWGUI.Emulation.Functions.EmulationCoreOptionFunctions.EditableFieldIds(SettingsDescriptionFunctions.Create(current));
        var options = new Dictionary<string, string>(current.Options, StringComparer.Ordinal);
        GWGUI.Emulation.Functions.EmulationCoreOptionFunctions.ApplyValues(options, values,
            optionKey => !editable.Contains(optionKey) || optionKey == SettingsConstants.AudioEnabled || FirmwareFunctions.IsField(optionKey));
        var result = current with
        {
            Firmwares = FirmwareFunctions.Apply(current, values),
            Options = options,
            AudioEnabled = values.TryGetValue(SettingsConstants.AudioEnabled, out var enabled) && editable.Contains(SettingsConstants.AudioEnabled) ? enabled == EmulationSettingConstants.Enabled : current.AudioEnabled
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration) => _engine.Adapter(Require(configuration)).GetRuntimeOptions(Require(configuration));
    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) => ConfigurationSummaryFunctions.Create(Require(configuration));
    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => InputSettingsFunctions.Describe(Require(configuration));
    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => InputSettingsFunctions.Apply(Require(configuration), settings);
    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => SaveConfigurationAsync(configuration, cancellationToken);
    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) => StorageSettingsFunctions.Describe(Require(configuration));
    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings) => StorageSettingsFunctions.Apply(Require(configuration), settings);

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false)).Cast<IEmulationConfiguration>().ToArray();
    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => new(_store.SaveAsync(Require(configuration), cancellationToken));
    public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default) => Adapter(machineId).GetInstallationAsync(_managementContext, cancellationToken);
    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.Adapter(Require(configuration)).GetInstallationAsync(_managementContext, cancellationToken);

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => _engine.Adapter(Require(configuration)).FindReleasesAsync(_managementContext, cancellationToken);

    public ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) =>
        _engine.Adapter(Require(configuration)).InstallAsync(_managementContext, release, progress, cancellationToken);

    public async ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var results = new List<EmulationEmulatorInstallation>();
        foreach (var adapter in _engine.Adapters(Require(configuration).MachineId))
            results.Add(await adapter.GetInstallationAsync(_managementContext, cancellationToken).ConfigureAwait(false));
        return results;
    }
    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current = Require(configuration);
        var adapter = _engine.Adapter(emulatorId);
        if (!adapter.Definition.MachineIds.Contains(current.MachineId))
            throw new ArgumentOutOfRangeException(nameof(emulatorId));
        var core = Enum.Parse<Emulator>(adapter.EmulatorKey);
        var options = EmulationCoreOptionFunctions.FilterForEmulator(CoreOptionValidationFunctions.ForCore(core).Select(option => option.Key), current.Options, SettingsConstants.AudioOutput, SettingsConstants.AudioLatency, VideoAudioSettingsConstants.AudioVolumeOption);
        var controllers = current.Input.Controllers?.Select(binding => InputSettingsFunctions.Peripherals(core).Contains(binding.Peripheral) ? binding : binding with { Peripheral = PeripheralCategory.Standard }).ToArray();
        var result = current with { Core = core, Options = options, Input = current.Input with { Controllers = controllers } };
        ConfigurationFunctions.Validate(result);
        return ValueTask.FromResult<IEmulationConfiguration>(result);
    }
    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default) => Adapter(machineId).FindReleasesAsync(_managementContext, cancellationToken);
    public ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => Adapter(machineId).InstallAsync(_managementContext, release, progress, cancellationToken);

    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default)
    {
        var current = Require(configuration);
        var adapter = _engine.Adapter(current);
        var corePath = await adapter.FindInstalledCorePathAsync(_managementContext, cancellationToken).ConfigureAwait(false) ?? throw GWGUI.Emulation.Services.EmulationErrorService.EmulatorNotInstalled(adapter.EmulatorId);
        var output = EmulationSdk.AudioProcessing.ResolveRuntimeOutput(current.Options, SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput);
        var latency = EmulationSdk.AudioProcessing.ResolveRuntimeLatency(current.Options, SettingsConstants.AudioLatency);
        var context = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath, () => services.CreateAudioOutput(output, latency),
            value => value.Folders.Saves ?? Path.Combine(services.StatesDirectory, EmulationModuleConstants.SavesDirectoryName, value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)));
        var storage = StorageSettingsFunctions.Describe(current);
        return new EmulationMachineRuntime(current, media =>
        {
            var selected = current with
            {
                Media = media.Select(item => EmulationMediaConversionFunctions.ToFairchild(item, current.Media)).ToArray()
            };
            return _engine.CreateMachine(selected, context);
        }, storage.AvailableDevices, storage.MountedMedia,
            ModelCatalog.All.Single(model => model.Id == current.MachineId).DisplayResourceKey, false);
    }

    private IEmulatorAdapter Adapter(string machineId) =>
        _engine.Adapters(ModelCatalog.Parse(machineId).ToString()).FirstOrDefault(adapter => adapter.EmulatorKey == ConfigurationFunctions.DefaultEmulator(ModelCatalog.Parse(machineId)).ToString())
            ?? throw new ArgumentOutOfRangeException(nameof(machineId));

    public string GetFirmwareDirectory(string machineId)
    {
        _ = ModelCatalog.Parse(machineId);
        return _firmwareDirectory;
    }

    public async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var current = Require(configuration);
        if (current.Model != ModelCatalog.Parse(machineId)) throw new ArgumentException(nameof(machineId));

        var directory = GetFirmwareDirectory(machineId);
        return await GWGUI.Emulation.Functions.EmulationFirmwareFunctions.ScanDirectoryAsync(directory,
            (path, token) => FirmwareFunctions.ScanFileAsync(path, current.Model, token), cancellationToken).ConfigureAwait(false);
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware)
    {
        var current = Require(configuration);
        var scanned = FirmwareFunctions.ScanFileAsync(firmware.Path, current.Model, CancellationToken.None).GetAwaiter().GetResult();
        var selected = FirmwareFunctions.Select(scanned);
        var result = current with { Firmwares = current.Firmwares.Where(item => item.Category != selected.Category).Append(selected).ToArray() };
        ConfigurationFunctions.Validate(result);
        return result;
    }
    private static MachineConfiguration Require(IEmulationConfiguration configuration) => configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
}
