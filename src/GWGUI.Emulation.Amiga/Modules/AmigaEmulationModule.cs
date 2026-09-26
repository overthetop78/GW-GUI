using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Modules;

public sealed class AmigaEmulationModule : IEmulationModule, IEmulationEmulatorManager,
    IEmulationFirmwareManager, IEmulationInputSettingsManager, IEmulationStorageSettingsManager,
    IEmulationModuleLocalization
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(AmigaEmulationModule).Assembly, "GWGUI.Emulation.Amiga.Resources.Emulation");

    public bool TryGetString(string key, System.Globalization.CultureInfo culture, out string value) =>
        Localization.TryGetString(key, culture, out value);

    private readonly ConfigurationStore _store;
    private readonly HttpClient _httpClient;
    private readonly string _coreDirectory;
    private readonly string _firmwareDirectory;
    private readonly Engine _engine = new();
    private EmulatorManagementContext EmulatorManagement(IEmulatorAdapter adapter) =>
        new(_httpClient, Path.Combine(_coreDirectory, adapter.EmulatorId));

    public AmigaEmulationModule(string configurationDirectory, string pathBase, HttpClient httpClient,
        string coreDirectory)
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
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(
        Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
        => _engine.TryHandleHostCommand(arguments, out exitCode);

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var model = ModelCatalog.Get(machineId);
        var visibility = DefaultVisibility with
        {
            Tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
            {
                EmulationMachineTab.Mouse => model.MouseButtonCount > 0,
                EmulationMachineTab.Controllers => model.ControllerPortCount > 0,
                EmulationMachineTab.Keyboard => model.HasKeyboard,
                _ => item.Value
            })
        };
        var current = configuration as MachineConfiguration
            ?? (MachineConfiguration)CreateConfiguration(machineId);
        return new EmulationMachineSettings(machineId, visibility,
            SettingsDescriptionFunctions.Create(model, current));
    }

    public IEmulationConfiguration CreateConfiguration(string machineId) =>
        MachineConfiguration.A500(string.Empty) with
        {
            Model = machineId,
            Id = Guid.NewGuid(),
            InitialDiskPath = null
        };

    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId)
    {
        if (configuration is not MachineConfiguration)
            throw new ArgumentException(nameof(configuration));
        var model = ModelCatalog.Get(machineId);
        return MachineConfiguration.A500(string.Empty) with
        {
            Model = model.Id,
            Options = new Dictionary<string, string>
            {
                [SettingsConstants.OptionModel] = model.BackendModel,
                [SettingsConstants.OptionVideoStandard] = SettingsDescriptionFunctionsConstants.PAL,
                [SettingsConstants.OptionFloppyMultidrive] = SettingsDescriptionFunctionsConstants.Disabled,
                [SettingsConstants.OptionFloppyWriteProtection] = SettingsDescriptionFunctionsConstants.Disabled
            },
            Id = Guid.NewGuid(),
            InitialDiskPath = null
        };
    }

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration,
        IReadOnlyDictionary<string, string?> values)
    {
        if (configuration is not MachineConfiguration amiga)
            throw new ArgumentException(nameof(configuration));
        var options = new Dictionary<string, string>(amiga.Options ?? new Dictionary<string, string>());
        foreach (var value in values)
        {
            if (value.Key is SettingsConstants.KickstartPath or SettingsConstants.ExtendedRomPath
                or SettingsConstants.RomKeyPath or SettingsConstants.AudioEnabled
                or SettingsConstants.CpuOriginalSpeed
                or SettingsConstants.CpuSpeed or SettingsConstants.AudioOutput
                or SettingsConstants.AudioLatency or SettingsConstants.AudioStereoSeparation
                or SettingsConstants.ParallelJoystickAdapter) continue;
            if (value.Value is null) options.Remove(value.Key);
            else options[value.Key] = value.Value;
        }
        if (values.TryGetValue(SettingsConstants.OptionSoundVolumeCd, out var cdVolume)
            && !string.IsNullOrWhiteSpace(cdVolume))
            options[SettingsConstants.OptionSoundVolumeCd] =
                cdVolume.TrimEnd(SettingsDescriptionFunctionsConstants.PercentSuffix)
                + SettingsDescriptionFunctionsConstants.PercentSuffix;
        if (values.GetValueOrDefault(SettingsConstants.CpuSpeed)?.Split('|') is [var throttle, var multiplier])
        {
            options[SettingsConstants.OptionCpuThrottle] = throttle;
            options[SettingsConstants.OptionCpuMultiplier] = multiplier;
        }
        var currentAudio = amiga.Audio ?? new AudioConfiguration();
        var hasOutput = values.TryGetValue(SettingsConstants.AudioOutput, out var output);
        var latency = int.TryParse(values.GetValueOrDefault(SettingsConstants.AudioLatency), out var latencyValue)
            ? latencyValue : currentAudio.LatencyMilliseconds;
        var stereo = int.TryParse(values.GetValueOrDefault(SettingsConstants.AudioStereoSeparation),
            out var stereoValue) ? stereoValue : currentAudio.StereoSeparation;
        var currentInput = amiga.Input ?? new InputConfiguration();
        var input = currentInput with
        {
            ParallelJoystickAdapterEnabled = values.TryGetValue(
                SettingsConstants.ParallelJoystickAdapter, out var parallelJoystickAdapter)
                    ? parallelJoystickAdapter == SettingsDescriptionFunctionsConstants.Enabled
                    : currentInput.ParallelJoystickAdapterEnabled
        };
        return amiga with
        {
            Options = options,
            KickstartPath = values.TryGetValue(SettingsConstants.KickstartPath, out var kickstartPath)
                ? kickstartPath ?? string.Empty : amiga.KickstartPath,
            ExtendedRomPath = values.TryGetValue(SettingsConstants.ExtendedRomPath, out var extendedRomPath)
                ? OptionalPath(extendedRomPath) : amiga.ExtendedRomPath,
            RomKeyPath = values.TryGetValue(SettingsConstants.RomKeyPath, out var romKeyPath)
                ? OptionalPath(romKeyPath) : amiga.RomKeyPath,
            AudioEnabled = values.TryGetValue(SettingsConstants.AudioEnabled, out var audioEnabled)
                ? audioEnabled == SettingsDescriptionFunctionsConstants.Enabled : amiga.AudioEnabled,
            Audio = currentAudio with
            {
                OutputDeviceId = hasOutput
                    ? string.IsNullOrWhiteSpace(output) ? null : output
                    : currentAudio.OutputDeviceId,
                LatencyMilliseconds = latency,
                Interpolation = options.GetValueOrDefault(SettingsConstants.OptionSoundInterpol) ?? currentAudio.Interpolation,
                Filter = options.GetValueOrDefault(SettingsConstants.OptionSoundFilter) ?? currentAudio.Filter,
                StereoSeparation = stereo
            },
            Input = input
        };
    }


    private static string? OptionalPath(string? path) =>
        string.IsNullOrWhiteSpace(path) ? null : path;

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration) =>
        new Dictionary<string, string>((configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration))).Options
            ?? new Dictionary<string, string>());

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) =>
        ConfigurationSummaryFunctions.Create(configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) =>
        InputSettingsFunctions.Describe(configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration,
        EmulationInputSettings settings) => InputSettingsFunctions.Apply(
        configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is MachineConfiguration amiga
        ? new ValueTask(_store.SaveAsync(amiga, cancellationToken))
        : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) =>
        StorageSettingsFunctions.Describe(configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration,
        EmulationStorageSettings settings) => StorageSettingsFunctions.Apply(
        configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(
        CancellationToken cancellationToken = default) =>
        (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false))
        .Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration amiga)
            return ValueTask.FromException(new ArgumentException(nameof(configuration)));
        ConfigurationValidationFunctions.ValidateForSave(amiga);
        return new ValueTask(_store.SaveAsync(amiga, cancellationToken));
    }

    public ValueTask DeleteConfigurationAsync(Guid configurationId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }

    public async ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId,
        CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var definition = EmulatorCatalog.GetAll(machineId).Single();
        var adapter = _engine.Adapter(definition.Id);
        return await adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId,
        CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var definition = EmulatorCatalog.GetAll(machineId).Single();
        var adapter = _engine.Adapter(definition.Id);
        return await adapter.FindReleasesAsync(EmulatorManagement(adapter), cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var definition = EmulatorCatalog.GetAll(machineId).Single();
        var adapter = _engine.Adapter(definition.Id);
        return await adapter.InstallAsync(EmulatorManagement(adapter), release, progress, cancellationToken)
            .ConfigureAwait(false);
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
        _ = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var entries = new FirmwareCatalog(GetFirmwareDirectory(machineId)).Scan()
            .Select(firmware => new EmulationFirmwareCandidate(firmware.Sha256, firmware.Path,
                firmware.Name ?? Path.GetFileName(firmware.Path), firmware.Version,
                FirmwareCompatibility(firmware, machineId), firmware.Type switch
                {
                    FirmwareType.Kickstart => SettingsConstants.KickstartPath,
                    FirmwareType.ExtendedRom => SettingsConstants.ExtendedRomPath,
                    FirmwareType.RomKey => SettingsConstants.RomKeyPath,
                    _ => null
                })).ToArray();
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>(entries);
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration,
        EmulationFirmwareCandidate firmware)
    {
        var amiga = configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        return firmware.DestinationFieldId switch
        {
            SettingsConstants.KickstartPath => amiga with { KickstartPath = firmware.Path },
            SettingsConstants.ExtendedRomPath => amiga with { ExtendedRomPath = firmware.Path },
            SettingsConstants.RomKeyPath => amiga with { RomKeyPath = firmware.Path },
            _ => throw new InvalidOperationException(nameof(firmware))
        };
    }

    private static EmulationFirmwareCompatibility FirmwareCompatibility(Firmware firmware, string machineId)
    {
        if (firmware.Type == FirmwareType.Unknown) return EmulationFirmwareCompatibility.Incompatible;
        if (firmware.Type == FirmwareType.RomKey) return EmulationFirmwareCompatibility.Compatible;
        if (!firmware.CompatibleModels.Contains(machineId, StringComparer.OrdinalIgnoreCase))
            return EmulationFirmwareCompatibility.Incompatible;
        return firmware.IsOfficial ? EmulationFirmwareCompatibility.Official
            : firmware.IsKnown ? EmulationFirmwareCompatibility.Compatible
            : EmulationFirmwareCompatibility.PartiallyCompatible;
    }

    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration,
        EmulationRuntimeServices services, CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration amiga)
            throw new ArgumentException(nameof(configuration));
        if (!File.Exists(amiga.KickstartPath))
            throw new EmulationMessageException(new EmulationMessage(
                EmulationMessageCategory.Firmware, EmulationMessageCode.FirmwareMissing,
                EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog));
        var runtime = await RuntimeMediaFunctions.PrepareConfigurationAsync(amiga,
            services.ConvertedMediaDirectory).ConfigureAwait(false);
        var emulator = _engine.Adapter(runtime);
        var corePath = await emulator.FindInstalledCorePathAsync(EmulatorManagement(emulator), cancellationToken)
            .ConfigureAwait(false)
            ?? throw new EmulationMessageException(new EmulationMessage(
                EmulationMessageCategory.Emulator, EmulationMessageCode.EmulatorNotInstalled,
                EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog,
                new EmulationEmulatorMessageContext(emulator.EmulatorId)));
        var audio = runtime.Audio ?? new AudioConfiguration();
        var creationContext = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
            value => Path.Combine(services.StatesDirectory,
                value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat),
                CoreDirectoryConstants.SavesDirectoryName));
        var storage = StorageSettingsFunctions.Describe(runtime);
        var devices = storage.AvailableDevices
            .Where(device => storage.ConfiguredSlots.Contains(device.Slot)).ToArray();
        var mounted = emulator.ResolveConfiguredMedia(runtime);
        return new EmulationMachineRuntime(runtime,
            CreateMachineFactory(_engine, runtime, creationContext), devices, mounted,
            MachineCatalog.All.First(machine => machine.Id == runtime.Model).DisplayResourceKey, true,
            (media, _) => RuntimeMediaFunctions.PrepareMediaAsync(media,
                services.ConvertedMediaDirectory));
    }

    private static Func<IReadOnlyList<EmulationMedia>, IEmulatedMachine> CreateMachineFactory(
        Engine engine,
        MachineConfiguration configuration,
        EmulatorCreationContext context) =>
        media => engine.CreateMachine(configuration with { Media = ToAmigaMedia(media) }, context);

    private static IReadOnlyList<MediaConfiguration> ToAmigaMedia(IEnumerable<EmulationMedia> media) =>
        media.Select(item => new MediaConfiguration(item.Path, item.Type switch
        {
            EmulationMediaType.Floppy => MediaCategory.Floppy,
            EmulationMediaType.HardDisk => MediaCategory.HardDrive,
            EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
            _ => throw new ArgumentOutOfRangeException(nameof(media), item.Type, null)
        }, IsReadOnly: item.IsReadOnly)).ToArray();
}
