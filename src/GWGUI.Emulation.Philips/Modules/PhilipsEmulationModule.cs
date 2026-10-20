using System.Globalization;
using System.IO;
using System.Net.Http;

namespace GWGUI.Emulation.Philips.Modules;

public sealed class PhilipsEmulationModule : IEmulationModule, IEmulationEmulatorManager,
    IEmulationInputSettingsManager, IEmulationStorageSettingsManager, IEmulationModuleLocalization,
    IEmulationFirmwareManager
{
    private static readonly EmulationModuleLocalization Localization = new(typeof(PhilipsEmulationModule).Assembly, EmulationModuleConstants.LocalizationResourceName);
    private readonly ConfigurationStore _store;
    private readonly EmulatorManagementContext _managementContext;
    private readonly Engine _engine = new();
    private readonly string _firmwareDirectory;

    public PhilipsEmulationModule(string configurationDirectory, string pathBase, HttpClient httpClient, string coreDirectory)
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _managementContext = new EmulatorManagementContext(httpClient, coreDirectory);
        _firmwareDirectory = Path.Combine(pathBase, EmulationPathConstants.RootDirectoryName, EmulationPathConstants.MachinesDirectoryName, EmulationModuleConstants.ModuleId, EmulationPathConstants.FirmwareDirectoryName);
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    // Brand image is independent of the machine photographs.
    public string BrandImageResourceName => $"{EmulationModuleConstants.AssetResourcePrefix}.philips.png";
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
            EmulationMachineTab.Keyboard => current.Family is MachineFamily.P2000 or MachineFamily.Videopac,
            EmulationMachineTab.Mouse => false,
            EmulationMachineTab.Rom => FirmwareFunctions.Roles(current.Model).Count > 0,
            EmulationMachineTab.Cpu => true,
            EmulationMachineTab.Ram => HardwareModelCatalog.Get(current.Model).MainMemoryBytes is not null,
            EmulationMachineTab.Controllers => ConfigurationFunctions.ControllerPortCount(current.Model)
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
        var editable = SettingsDescriptionFunctions.Create(current).SelectMany(block => block.Fields).Where(field => field.IsEnabled && field.IsVisible && field.Editor != EmulationSettingsEditor.Information).Select(field => field.Id).ToHashSet(StringComparer.Ordinal);
        var options = new Dictionary<string, string>(current.Options, StringComparer.Ordinal);
        foreach (var value in values)
        {
            if (!editable.Contains(value.Key) || value.Key == SettingsConstants.AudioEnabled || FirmwareFunctions.IsField(value.Key)) continue;
            if (value.Value is null) options.Remove(value.Key);
            else options[value.Key] = value.Value;
        }
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
        var allowed = CoreOptionValidationFunctions.ForCore(core).Select(option => option.Key).ToHashSet(StringComparer.Ordinal);
        var options = current.Options.Where(option => allowed.Contains(option.Key)
            || option.Key == SettingsConstants.AudioOutput || option.Key == SettingsConstants.AudioLatency
            || option.Key == VideoAudioSettingsConstants.AudioVolumeOption).ToDictionary(option => option.Key, option => option.Value);
        var controllers = current.Input.Controllers?.Select(binding => InputSettingsFunctions.Peripherals(current.Model, core).Contains(binding.Peripheral)
            ? binding
            : binding with { Peripheral = current.Family == MachineFamily.Cdi ? PeripheralCategory.CdiGamepad : PeripheralCategory.Joystick }).ToArray();
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
        var corePath = await adapter.FindInstalledCorePathAsync(_managementContext, cancellationToken).ConfigureAwait(false) ?? throw new EmulationMessageException(new EmulationMessage(
                EmulationMessageCategory.Emulator, EmulationMessageCode.EmulatorNotInstalled,
                EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog,
                new EmulationEmulatorMessageContext(adapter.EmulatorId)));
        var output = EmulationAudioFunctions.ResolveRuntimeOutput(current.Options, SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput);
        var latency = EmulationAudioFunctions.ResolveRuntimeLatency(current.Options, SettingsConstants.AudioLatency);
        var context = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath, () => services.CreateAudioOutput(output, latency),
            value => value.Folders.Saves ?? Path.Combine(services.StatesDirectory, EmulationModuleConstants.SavesDirectoryName, value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)));
        var storage = StorageSettingsFunctions.Describe(current);
        return new EmulationMachineRuntime(current, media =>
        {
            var selected = current with
            {
                Media = media.Select(item => EmulationMediaConversionFunctions.ToPhilips(item, current.Media)).ToArray()
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
        Directory.CreateDirectory(directory);
        var candidates = new List<EmulationFirmwareCandidate>();
        foreach (var path in Directory.EnumerateFiles(directory).Order(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            candidates.Add(await FirmwareFunctions.ScanFileAsync(path, current.Model, current.Core, cancellationToken).ConfigureAwait(false));
        }
        return candidates;
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware)
    {
        var current = Require(configuration);
        var scanned = FirmwareFunctions.ScanFileAsync(firmware.Path, current.Model, current.Core, CancellationToken.None).GetAwaiter().GetResult();
        var selected = FirmwareFunctions.Select(scanned);
        var result = current with { Firmwares = current.Firmwares.Where(item => item.Category != selected.Category).Append(selected).ToArray() };
        ConfigurationFunctions.Validate(result);
        return result;
    }
    private static MachineConfiguration Require(IEmulationConfiguration configuration) => configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
}
