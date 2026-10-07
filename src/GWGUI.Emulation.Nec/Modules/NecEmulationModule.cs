using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngine.Constants;
using GWGUI.Emulation.Nec.Common.Machines.CoreGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcFx.Constants;
using System.IO;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;

namespace GWGUI.Emulation.Nec.Modules;

public sealed class NecEmulationModule : IEmulationModule, IEmulationEmulatorManager,
    IEmulationFirmwareManager, IEmulationInputSettingsManager, IEmulationStorageSettingsManager,
    IEmulationModuleLocalization
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NecEmulationModule).Assembly, "GWGUI.Emulation.Nec.Resources.Emulation");
    private readonly ConfigurationStore _store;
    private readonly HttpClient _httpClient;
    private readonly string _coreDirectory;
    private readonly string _firmwareDirectory;
    private readonly Engine _engine = new();
    private EmulatorManagementContext EmulatorManagement(IEmulatorAdapter adapter) =>
        new(_httpClient, Path.Combine(_coreDirectory, adapter.EmulatorId));

    public NecEmulationModule(string configurationDirectory, string pathBase,
        HttpClient httpClient, string coreDirectory)
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _httpClient = httpClient;
        _coreDirectory = coreDirectory;
        _firmwareDirectory = Path.Combine(pathBase, EmulationPathConstants.RootDirectoryName,
            EmulationPathConstants.MachinesDirectoryName, FirmwareConstants.DirectoryName,
            EmulationPathConstants.FirmwareDirectoryName);
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    public string BrandImageResourceName => EmulationModuleConstants.BrandAssetResourceName;
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
            EmulationMachineTab.Storage => model.SupportsCdDrive || model.SupportsCartridgeSlot
                || model.MaximumFloppyDriveCount > 0,
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
            Controllers: Enumerable.Range(InputSettingsFunctionsConstants.FirstPortIndex,
                model.ControllerPortCount)
                .Select(index => index == InputSettingsFunctionsConstants.FirstPortIndex
                    ? ControllerCatalog.Default(model)
                    : ControllerType.None).ToArray(),
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
        var nintendo = RequireConfiguration(configuration);
        var options = new Dictionary<string, string>(nintendo.Options
            ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        var firmwareSlots = FirmwareFunctions.Slots(nintendo.EmulatorId);
        foreach (var item in values)
        {
            if (firmwareSlots.Any(slot => slot.FieldId == item.Key)) continue;
            if (item.Key is SettingsConstants.Model or SettingsConstants.Emulator
                or SettingsConstants.AudioEnabled or SettingsConstants.AudioOutput
                or SettingsConstants.AudioLatency or SettingsConstants.FirmwarePath
                or SettingsConstants.Ram or SettingsConstants.FirmwareIntegrated
                || item.Key.StartsWith(
                    SettingsConstants.Model + ".", StringComparison.Ordinal)) continue;
            if (item.Value is null) options.Remove(item.Key);
            else options[item.Key] = item.Value;
        }
        var audio = nintendo.Audio ?? new AudioConfiguration();
        return FirmwareFunctions.Apply(nintendo, values) with
        {
            Options = options,
            FirmwarePath = values.TryGetValue(SettingsConstants.FirmwarePath, out var firmware)
                ? string.IsNullOrWhiteSpace(firmware) ? null : firmware : nintendo.FirmwarePath,
            AudioEnabled = values.TryGetValue(SettingsConstants.AudioEnabled, out var enabled)
                ? enabled == SettingsDescriptionFunctionsConstants.Enabled : nintendo.AudioEnabled,
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
        configuration is MachineConfiguration nintendo
            ? nintendo.Options ?? new Dictionary<string, string>()
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

    public async ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration machine)
            throw new ArgumentException(nameof(configuration));
        await SaveMachineAsync(machine, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(
        CancellationToken cancellationToken = default) =>
        (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false))
        .Cast<IEmulationConfiguration>().ToArray();

    public async ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration machine)
            throw new ArgumentException(nameof(configuration));
        ConfigurationValidationFunctions.ValidateForSave(machine);
        await SaveMachineAsync(machine, cancellationToken).ConfigureAwait(false);
    }

    private async Task SaveMachineAsync(MachineConfiguration machine,
        CancellationToken cancellationToken)
    {
        await _store.SaveAsync(machine, cancellationToken).ConfigureAwait(false);
        if (machine.Model == PcFxMachineConstants.Id
            && machine.EmulatorId == BeetlePcfxConstants.Id)
            PcFxBackupMemoryFunctions.EnsureInternalFile(_store.MemoryDirectory(machine.Id));
    }

    public ValueTask DeleteConfigurationAsync(Guid configurationId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }

    public string GetFirmwareDirectory(string machineId)
    {
        _ = ModelCatalog.Get(machineId);
        return _firmwareDirectory;
    }

    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId,
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current = RequireConfiguration(configuration);
        if (!string.Equals(current.Model, machineId, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(nameof(configuration));
        if (FirmwareFunctions.Slots(current.EmulatorId).Count != 0)
            return ValueTask.FromResult(FirmwareFunctions.Scan(current, GetFirmwareDirectory(machineId)));
        if (!ModelCatalog.Get(machineId).SupportsCdDrive)
            return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>([]);
        var candidates = new FirmwareCatalog(GetFirmwareDirectory(machineId)).Scan()
            .Select(firmware =>
            {
                var compatible = firmware.CompatibleModels.Contains(machineId,
                    StringComparer.OrdinalIgnoreCase);
                return new EmulationFirmwareCandidate(firmware.Sha256, firmware.Path,
                    firmware.Name ?? Path.GetFileName(firmware.Path), firmware.Version,
                    compatible ? !firmware.IsVerified
                        ? EmulationFirmwareCompatibility.Unknown
                        : firmware.IsOfficial ? EmulationFirmwareCompatibility.Official
                            : EmulationFirmwareCompatibility.Compatible
                        : firmware.IsKnown ? EmulationFirmwareCompatibility.Incompatible
                            : EmulationFirmwareCompatibility.Unknown,
                    compatible || !firmware.IsKnown ? SettingsConstants.FirmwarePath : null);
            }).ToArray();
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>(candidates);
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration,
        EmulationFirmwareCandidate firmware)
    {
        var current = RequireConfiguration(configuration);
        if (FirmwareFunctions.Slots(current.EmulatorId).Any(slot => slot.FieldId == firmware.DestinationFieldId))
        {
            if (!File.Exists(firmware.Path)) throw new FileNotFoundException(null, firmware.Path);
            return FirmwareFunctions.Apply(current, new Dictionary<string, string?>
                { [firmware.DestinationFieldId!] = Path.GetFullPath(firmware.Path) });
        }
        if (firmware.DestinationFieldId != SettingsConstants.FirmwarePath)
            throw new InvalidOperationException(nameof(firmware));
        var inspected = FirmwareCatalog.Inspect(firmware.Path);
        if (inspected.IsKnown && !inspected.CompatibleModels.Contains(current.Model,
                StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(nameof(firmware));
        return current with { FirmwarePath = inspected.Path };
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
        var nintendo = RequireConfiguration(configuration);
        var installations = new List<EmulationEmulatorInstallation>();
        foreach (var emulator in EmulatorCatalog.GetAll(nintendo.Model))
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
        var nintendo = RequireConfiguration(configuration);
        if (!EmulatorCatalog.GetAll(nintendo.Model).Any(item => item.Id == emulatorId))
            return ValueTask.FromException<IEmulationConfiguration>(
                new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null));
        return ValueTask.FromResult<IEmulationConfiguration>(nintendo with { EmulatorId = emulatorId });
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
        if (configuration is not MachineConfiguration nintendo)
            throw new ArgumentException(nameof(configuration));
        var adapter = _engine.Adapter(nintendo);
        var corePath = await adapter.FindInstalledCorePathAsync(EmulatorManagement(adapter), cancellationToken)
            .ConfigureAwait(false) ?? throw new EmulationMessageException(new EmulationMessage(
                EmulationMessageCategory.Emulator, EmulationMessageCode.EmulatorNotInstalled,
                EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog,
                new EmulationEmulatorMessageContext(adapter.EmulatorId)));
        var audio = nintendo.Audio ?? new AudioConfiguration();
        var context = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
            value => value.Model == PcFxMachineConstants.Id ? _store.MemoryDirectory(value.Id)
                : Path.Combine(services.StatesDirectory,
                    value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat),
                    CoreDirectoryConstants.SavesDirectoryName),
            GetFirmwareDirectory(nintendo.Model));
        var storage = StorageSettingsFunctions.Describe(nintendo);
        var mounted = adapter.ResolveConfiguredMedia(nintendo);
        return new EmulationMachineRuntime(nintendo,
            media => _engine.CreateMachine(WithMedia(nintendo, media), context),
            storage.AvailableDevices.Where(device => storage.ConfiguredSlots.Contains(device.Slot)).ToArray(), mounted,
            MachineConfigurationConstants.ResourcePrefix + nintendo.Model,
            SupportsPointerCapture: ModelCatalog.Get(nintendo.Model).MouseButtonCount > 0);
    }

    private static MachineConfiguration WithMedia(MachineConfiguration configuration,
        IEnumerable<EmulationMedia> media) => configuration with
    {
        Media = media.Select((item, index) => new MediaConfiguration(item.Path,
            item.Type switch
            {
                EmulationMediaType.Cartridge => MediaCategory.Cartridge,
                EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
                EmulationMediaType.Floppy => MediaCategory.Floppy,
                EmulationMediaType.HardDisk => MediaCategory.HardDisk,
                _ => throw new ArgumentOutOfRangeException(nameof(media), item.Type, null)
            }, IsReadOnly: item.IsReadOnly, IsInserted: item.IsInserted,
            MountOrder: index, SlotIndex: item.Slot.Index)).ToArray()
    };

    private static string DefaultEmulatorId(string machineId) =>
        EmulatorCatalog.DefaultFor(machineId);

    private static MachineConfiguration RequireConfiguration(IEmulationConfiguration configuration) =>
        configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
}

