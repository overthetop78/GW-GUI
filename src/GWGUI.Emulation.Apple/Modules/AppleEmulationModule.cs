using System.Globalization;
using System.IO;

namespace GWGUI.Emulation.Apple.Modules;

public sealed class AppleEmulationModule : IEmulationModule, IEmulationEmulatorManager,
    IEmulationInputSettingsManager, IEmulationStorageSettingsManager, IEmulationModuleLocalization,
    IEmulationFirmwareManager
{
    private static readonly EmulationModuleLocalization Localization = new(typeof(AppleEmulationModule).Assembly, EmulationModuleConstants.LocalizationResourceName);
    private readonly ConfigurationStore _store;
    private readonly EmulatorManagementContext _managementContext;
    private readonly Engine _engine = new();
    private readonly string _firmwareDirectory;

    public AppleEmulationModule(string configurationDirectory, string pathBase, HttpClient httpClient, string coreDirectory)
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _managementContext = new EmulatorManagementContext(httpClient, coreDirectory);
        _firmwareDirectory = Path.Combine(pathBase, EmulationPathConstants.RootDirectoryName,
            EmulationPathConstants.MachinesDirectoryName, FirmwareConstants.DirectoryName,
            EmulationPathConstants.FirmwareDirectoryName, FirmwareConstants.MacintoshDirectoryName);
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    public string BrandImageResourceName => $"{EmulationModuleConstants.AssetResourcePrefix}.apple.png";
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
            _ when current.Core == Emulator.None => item.Key is EmulationMachineTab.General or EmulationMachineTab.Cpu or EmulationMachineTab.Ram,
            EmulationMachineTab.Mouse => current.Family == MachineFamily.Macintosh || ConfigurationFunctions.SupportsRuntime(model),
            EmulationMachineTab.Controllers => ConfigurationFunctions.ControllerPortCount(model)
                > BufferConstants.EmptyCollectionCount,
            EmulationMachineTab.Storage => ConfigurationFunctions.SupportsRuntime(model),
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
            if (!editable.Contains(value.Key) || value.Key is SettingsConstants.AudioEnabled
                or SettingsConstants.SharedFolder or SettingsConstants.FloppyFolder
                or SettingsConstants.HardDiskFolder or SettingsConstants.Firmware
                or SettingsConstants.StateFolder or SettingsConstants.CaptureFolder) continue;
            if (value.Value is null) options.Remove(value.Key);
            else options[value.Key] = value.Value;
        }
        var folders = current.Folders with
        {
            Shared = Folder(SettingsConstants.SharedFolder, current.Folders.Shared),
            Floppies = Folder(SettingsConstants.FloppyFolder, current.Folders.Floppies),
            HardDisks = Folder(SettingsConstants.HardDiskFolder, current.Folders.HardDisks),
            States = Folder(SettingsConstants.StateFolder, current.Folders.States),
            Captures = Folder(SettingsConstants.CaptureFolder, current.Folders.Captures)
        };
        var result = current with
        {
            Firmwares = values.TryGetValue(SettingsConstants.Firmware, out var firmwarePath) && editable.Contains(SettingsConstants.Firmware) ? string.IsNullOrWhiteSpace(firmwarePath) ? [] : [new FirmwareConfiguration(FirmwareCategory.SystemRom, firmwarePath, true)] : current.Firmwares,
            Options = options, Folders = folders,
            AudioEnabled = values.TryGetValue(SettingsConstants.AudioEnabled, out var enabled) ? enabled == EmulationSettingConstants.Enabled : current.AudioEnabled
        };
        if (_engine.Adapter(result) is IEmulatorSettingsApplication settingsAdapter)
            result = settingsAdapter.ApplySettings(result, values.Where(value => editable.Contains(value.Key)).ToDictionary(value => value.Key, value => value.Value, StringComparer.Ordinal));
        ConfigurationFunctions.Validate(result);
        return result;
        string? Folder(string id, string? original) => values.TryGetValue(id, out var value) ? value : original;
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
        return ValueTask.FromResult<IEmulationConfiguration>(current with { Core = Enum.Parse<Emulator>(adapter.EmulatorKey) });
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
        var output = current.Options.GetValueOrDefault(SettingsConstants.AudioOutput);
        if (output == SettingsConstants.DefaultOutput) output = null;
        var latency = current.Options.TryGetValue(SettingsConstants.AudioLatency, out var selectedLatency) && int.TryParse(selectedLatency, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : SettingsConstants.DefaultLatency;
        var context = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath, () => services.CreateAudioOutput(output, latency),
            value => Path.Combine(services.StatesDirectory, value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)));
        var storage = StorageSettingsFunctions.Describe(current);
        return new EmulationMachineRuntime(current, media =>
        {
            var selected = current with
            {
                Media = media.Select(item => EmulationMediaConversionFunctions.ToApple(item, current.Media)).ToArray()
            };
            return _engine.CreateMachine(selected, context);
        }, storage.AvailableDevices, storage.MountedMedia,
            HardwareModelCatalog.Get(current.Model).DisplayNameResourceKey, InputSettingsFunctions.SupportsPointer(current));
    }

    private IEmulatorAdapter Adapter(string machineId) => _engine.Adapters(ModelCatalog.Parse(machineId).ToString()).SingleOrDefault() ?? throw new ArgumentOutOfRangeException(nameof(machineId));

    public string GetFirmwareDirectory(string machineId)
    {
        _ = ModelCatalog.Parse(machineId);
        return _firmwareDirectory;
    }

    public async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var current = Require(configuration);
        if (current.Model != ModelCatalog.Parse(machineId)) throw new ArgumentException(nameof(machineId));
        if (_engine.SelectedAdapter(current) is not IEmulatorFirmwareAdapter firmwareAdapter || !ConfigurationFunctions.SupportsRuntime(current.Model)) return [];
        var directory = GetFirmwareDirectory(machineId);
        Directory.CreateDirectory(directory);
        var candidates = new List<EmulationFirmwareCandidate>();
        foreach (var path in Directory.EnumerateFiles(directory).Order(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            candidates.Add(await FirmwareFunctions.ScanFileAsync(firmwareAdapter, current, path, cancellationToken).ConfigureAwait(false));
        }
        return candidates;
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware)
    {
        var current = Require(configuration);
        if (_engine.SelectedAdapter(current) is not IEmulatorFirmwareAdapter firmwareAdapter || !ConfigurationFunctions.SupportsRuntime(current.Model))
            throw new ArgumentException(nameof(configuration));
        var scanned = FirmwareFunctions.ScanFileAsync(firmwareAdapter, current, firmware.Path, CancellationToken.None).GetAwaiter().GetResult();
        var result = current with { Firmwares = [FirmwareFunctions.Select(scanned)] };
        ConfigurationFunctions.Validate(result);
        return result;
    }
    private static MachineConfiguration Require(IEmulationConfiguration configuration) => configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
}
