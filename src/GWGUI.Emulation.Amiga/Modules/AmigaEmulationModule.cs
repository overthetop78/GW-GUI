using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Modules;

public sealed class AmigaEmulationModule : IEmulationModule, IEmulationEmulatorManager,
    IEmulationFirmwareManager, IEmulationInputSettingsManager, IEmulationStorageSettingsManager
{
    private readonly AmigaConfigurationStore _store;
    private readonly HttpClient _httpClient;
    private readonly string _coreDirectory;
    private readonly string _firmwareDirectory;
    private readonly AmigaEngine _engine = new();
    private IReadOnlyDictionary<string, AmigaCoreRelease> _availableReleases =
        new Dictionary<string, AmigaCoreRelease>(StringComparer.Ordinal);

    public AmigaEmulationModule(string configurationDirectory, string pathBase, HttpClient httpClient,
        string coreDirectory)
    {
        _store = new AmigaConfigurationStore(configurationDirectory, pathBase);
        _httpClient = httpClient;
        _coreDirectory = coreDirectory;
        _firmwareDirectory = Path.Combine(pathBase, EmulationPathConstants.RootDirectoryName,
            EmulationPathConstants.MachinesDirectoryName, AmigaFirmwareConstants.DirectoryName,
            EmulationPathConstants.FirmwareDirectoryName);
    }

    public string Id => AmigaEmulationModuleConstants.Amiga;
    public string DisplayResourceKey => AmigaEmulationModuleConstants.ResourceFamilyAmiga;
    public IReadOnlyList<EmulationMachineDefinition> Machines => AmigaMachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(
        Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is not [AmigaEmulationModuleConstants.AmigaCoreHost, var pipeName, var videoMapName]) return false;
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();
        AmigaCoreHost.Run(pipeName, videoMapName);
        return true;
    }

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var model = AmigaModelCatalog.Get(machineId);
        var visibility = DefaultVisibility with
        {
            Tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
            {
                EmulationMachineTab.Mouse => model.MouseButtonCount > 0,
                EmulationMachineTab.Controllers => model.ControllerPortCount > 0,
                _ => item.Value
            })
        };
        var current = configuration as AmigaMachineConfiguration
            ?? (AmigaMachineConfiguration)CreateConfiguration(machineId);
        return new EmulationMachineSettings(machineId, visibility,
            AmigaSettingsDescriptionFunctions.Create(model, current));
    }

    public IEmulationConfiguration CreateConfiguration(string machineId) =>
        AmigaMachineConfiguration.A500(string.Empty) with
        {
            Model = machineId,
            Id = Guid.NewGuid(),
            InitialDiskPath = null
        };

    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId)
    {
        if (configuration is not AmigaMachineConfiguration)
            throw new ArgumentException(nameof(configuration));
        var model = AmigaModelCatalog.Get(machineId);
        return AmigaMachineConfiguration.A500(string.Empty) with
        {
            Model = model.Id,
            Options = new Dictionary<string, string>
            {
                [AmigaEmulationModuleConstants.OptionModel] = model.BackendModel,
                [AmigaEmulationModuleConstants.OptionVideoStandard] = AmigaEmulationModuleConstants.PAL,
                [AmigaEmulationModuleConstants.OptionFloppyMultidrive] = AmigaEmulationModuleConstants.Disabled,
                [AmigaEmulationModuleConstants.OptionFloppyWriteProtection] = AmigaEmulationModuleConstants.Disabled
            },
            Id = Guid.NewGuid(),
            InitialDiskPath = null
        };
    }

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration,
        IReadOnlyDictionary<string, string?> values)
    {
        if (configuration is not AmigaMachineConfiguration amiga)
            throw new ArgumentException(nameof(configuration));
        var options = new Dictionary<string, string>(amiga.Options ?? new Dictionary<string, string>());
        foreach (var value in values)
        {
            if (value.Key is AmigaSettingsConstants.KickstartPath or AmigaSettingsConstants.ExtendedRomPath
                or AmigaSettingsConstants.RomKeyPath or AmigaSettingsConstants.AudioEnabled
                or AmigaSettingsConstants.VideoRenderer or AmigaSettingsConstants.CpuOriginalSpeed
                or AmigaSettingsConstants.CpuSpeed or AmigaSettingsConstants.AudioOutput
                or AmigaSettingsConstants.AudioLatency or AmigaSettingsConstants.AudioStereoSeparation
                or AmigaSettingsConstants.ParallelJoystickAdapter) continue;
            if (value.Value is null) options.Remove(value.Key);
            else options[value.Key] = value.Value;
        }
        if (values.TryGetValue(AmigaEmulationModuleConstants.OptionSoundVolumeCd, out var cdVolume)
            && !string.IsNullOrWhiteSpace(cdVolume))
            options[AmigaEmulationModuleConstants.OptionSoundVolumeCd] = cdVolume.TrimEnd('%') + AmigaEmulationModuleConstants.Value;
        if (values.GetValueOrDefault(AmigaSettingsConstants.CpuSpeed)?.Split('|') is [var throttle, var multiplier])
        {
            options[AmigaEmulationModuleConstants.OptionCpuThrottle] = throttle;
            options[AmigaEmulationModuleConstants.OptionCpuMultiplier] = multiplier;
        }
        var renderer = values.TryGetValue(AmigaSettingsConstants.VideoRenderer, out var rendererValue)
            && Enum.TryParse<EmulationVideoRenderer>(rendererValue, out var selectedRenderer)
                ? selectedRenderer : amiga.VideoRenderer;
        var currentAudio = amiga.Audio ?? new AmigaAudioConfiguration();
        var output = values.GetValueOrDefault(AmigaSettingsConstants.AudioOutput);
        var latency = int.TryParse(values.GetValueOrDefault(AmigaSettingsConstants.AudioLatency), out var latencyValue)
            ? latencyValue : currentAudio.LatencyMilliseconds;
        var stereo = int.TryParse(values.GetValueOrDefault(AmigaSettingsConstants.AudioStereoSeparation),
            out var stereoValue) ? stereoValue : currentAudio.StereoSeparation;
        var input = (amiga.Input ?? new AmigaInputConfiguration()) with
        {
            ParallelJoystickAdapterEnabled =
                values.GetValueOrDefault(AmigaSettingsConstants.ParallelJoystickAdapter) == AmigaEmulationModuleConstants.Enabled
        };
        return amiga with
        {
            Options = options,
            KickstartPath = values.GetValueOrDefault(AmigaSettingsConstants.KickstartPath) ?? string.Empty,
            ExtendedRomPath = OptionalPath(values.GetValueOrDefault(AmigaSettingsConstants.ExtendedRomPath)),
            RomKeyPath = OptionalPath(values.GetValueOrDefault(AmigaSettingsConstants.RomKeyPath)),
            AudioEnabled = values.GetValueOrDefault(AmigaSettingsConstants.AudioEnabled) == AmigaEmulationModuleConstants.Enabled,
            Audio = currentAudio with
            {
                OutputDeviceId = string.IsNullOrWhiteSpace(output) ? null : output,
                LatencyMilliseconds = latency,
                Interpolation = options.GetValueOrDefault(AmigaEmulationModuleConstants.OptionSoundInterpol) ?? currentAudio.Interpolation,
                Filter = options.GetValueOrDefault(AmigaEmulationModuleConstants.OptionSoundFilter) ?? currentAudio.Filter,
                StereoSeparation = stereo
            },
            VideoRenderer = renderer,
            Input = input
        };
    }

    private static string? OptionalPath(string? path) =>
        string.IsNullOrWhiteSpace(path) ? null : path;

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) =>
        AmigaConfigurationSummaryFunctions.Create(configuration as AmigaMachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) =>
        AmigaInputSettingsFunctions.Describe(configuration as AmigaMachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration,
        EmulationInputSettings settings) => AmigaInputSettingsFunctions.Apply(
        configuration as AmigaMachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is AmigaMachineConfiguration amiga
        ? new ValueTask(_store.SaveAsync(amiga, cancellationToken))
        : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) =>
        AmigaStorageSettingsFunctions.Describe(configuration as AmigaMachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration,
        EmulationStorageSettings settings) => AmigaStorageSettingsFunctions.Apply(
        configuration as AmigaMachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(
        CancellationToken cancellationToken = default) =>
        (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false))
        .Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration is not AmigaMachineConfiguration amiga)
            return ValueTask.FromException(new ArgumentException(nameof(configuration)));
        AmigaConfigurationValidationFunctions.ValidateForSave(amiga);
        return new ValueTask(_store.SaveAsync(amiga, cancellationToken));
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
        cancellationToken.ThrowIfCancellationRequested();
        _ = AmigaModelCatalog.Get(machineId);
        var version = new AmigaCoreReleaseService(_httpClient, _coreDirectory).GetInstalledVersion();
        return ValueTask.FromResult(new EmulationEmulatorInstallation(AmigaEmulationModuleConstants.Puae, version));
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId,
        CancellationToken cancellationToken = default)
    {
        _ = AmigaModelCatalog.Get(machineId);
        var releases = await new AmigaCoreReleaseService(_httpClient, _coreDirectory)
            .GetAvailableAsync(cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id, item.DisplayName, item.Id,
            item.IsRequired)).ToArray();
    }

    public async ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        _ = AmigaModelCatalog.Get(machineId);
        if (!_availableReleases.TryGetValue(release.Id, out var selected))
            throw new ArgumentException(nameof(release));
        return await new AmigaCoreReleaseService(_httpClient, _coreDirectory)
            .InstallAsync(selected, progress, cancellationToken).ConfigureAwait(false);
    }

    public string GetFirmwareDirectory(string machineId)
    {
        _ = AmigaModelCatalog.Get(machineId);
        return _firmwareDirectory;
    }

    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId,
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = configuration as AmigaMachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var entries = new AmigaFirmwareCatalog(GetFirmwareDirectory(machineId)).Scan()
            .Select(firmware => new EmulationFirmwareCandidate(firmware.Sha256, firmware.Path,
                firmware.Name ?? Path.GetFileName(firmware.Path), firmware.Version,
                FirmwareCompatibility(firmware, machineId), firmware.Type switch
                {
                    AmigaFirmwareType.Kickstart => AmigaSettingsConstants.KickstartPath,
                    AmigaFirmwareType.ExtendedRom => AmigaSettingsConstants.ExtendedRomPath,
                    AmigaFirmwareType.RomKey => AmigaSettingsConstants.RomKeyPath,
                    _ => null
                })).ToArray();
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>(entries);
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration,
        EmulationFirmwareCandidate firmware)
    {
        var amiga = configuration as AmigaMachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        return firmware.DestinationFieldId switch
        {
            AmigaSettingsConstants.KickstartPath => amiga with { KickstartPath = firmware.Path },
            AmigaSettingsConstants.ExtendedRomPath => amiga with { ExtendedRomPath = firmware.Path },
            AmigaSettingsConstants.RomKeyPath => amiga with { RomKeyPath = firmware.Path },
            _ => throw new InvalidOperationException(nameof(firmware))
        };
    }

    private static EmulationFirmwareCompatibility FirmwareCompatibility(AmigaFirmware firmware, string machineId)
    {
        if (firmware.Type == AmigaFirmwareType.Unknown) return EmulationFirmwareCompatibility.Incompatible;
        if (firmware.Type == AmigaFirmwareType.RomKey) return EmulationFirmwareCompatibility.Compatible;
        if (!firmware.CompatibleModels.Contains(machineId, StringComparer.OrdinalIgnoreCase))
            return EmulationFirmwareCompatibility.Incompatible;
        return firmware.IsOfficial ? EmulationFirmwareCompatibility.Official
            : firmware.IsKnown ? EmulationFirmwareCompatibility.Compatible
            : EmulationFirmwareCompatibility.PartiallyCompatible;
    }

    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration,
        EmulationRuntimeServices services, CancellationToken cancellationToken = default)
    {
        if (configuration is not AmigaMachineConfiguration amiga)
            throw new ArgumentException(nameof(configuration));
        if (!File.Exists(amiga.KickstartPath))
            throw new FileNotFoundException(AmigaEmulationModuleConstants.Kickstart, amiga.KickstartPath);
        var runtime = await AmigaRuntimeMediaFunctions.PrepareConfigurationAsync(amiga,
            services.ConvertedMediaDirectory).ConfigureAwait(false);
        var corePath = await new AmigaCoreProvider(_httpClient, _coreDirectory)
            .FindInstalledPathAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new EmulationMessageException(new EmulationMessage(
                EmulationMessageCategory.Emulator, EmulationMessageCode.EmulatorNotInstalled,
                EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog,
                new EmulationEmulatorMessageContext(AmigaEmulationModuleConstants.Puae)));
        var audio = runtime.Audio ?? new AmigaAudioConfiguration();
        var creationContext = new AmigaMachineCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
            value => Path.Combine(services.StatesDirectory, value.Id.ToString(AmigaEmulationModuleConstants.N), AmigaEmulationModuleConstants.Saves));
        var storage = AmigaStorageSettingsFunctions.Describe(runtime);
        var devices = storage.AvailableDevices
            .Where(device => storage.ConfiguredSlots.Contains(device.Slot)).ToArray();
        var mounted = EmulationMediaConversionFunctions.ToCommon(
            AmigaExternalCore.ResolveConfiguredMedia(runtime));
        return new EmulationMachineRuntime(runtime,
            CreateMachineFactory(_engine, runtime, creationContext), devices, mounted,
            AmigaMachineCatalog.All.First(machine => machine.Id == runtime.Model).DisplayResourceKey, true,
            (media, _) => AmigaRuntimeMediaFunctions.PrepareMediaAsync(media,
                services.ConvertedMediaDirectory));
    }

    private static Func<IReadOnlyList<EmulationMedia>, IEmulatedMachine> CreateMachineFactory(
        AmigaEngine engine,
        AmigaMachineConfiguration configuration,
        AmigaMachineCreationContext context) =>
        media => engine.CreateMachine(configuration with { Media = ToAmigaMedia(media) }, context);

    private static IReadOnlyList<AmigaMediaConfiguration> ToAmigaMedia(IEnumerable<EmulationMedia> media) =>
        media.Select(item => new AmigaMediaConfiguration(item.Path, item.Type switch
        {
            EmulationMediaType.Floppy => AmigaMediaCategory.Floppy,
            EmulationMediaType.HardDisk => AmigaMediaCategory.HardDrive,
            EmulationMediaType.CompactDisc => AmigaMediaCategory.CompactDisc,
            _ => throw new ArgumentOutOfRangeException(nameof(media), item.Type, null)
        }, IsReadOnly: item.IsReadOnly)).ToArray();
}
