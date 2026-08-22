using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Cores;

namespace GWGUI.Emulation.Amiga;

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
        _firmwareDirectory = Path.Combine(pathBase, "Emulation", "Firmware", "Amiga");
    }

    public string Id => "amiga";
    public string DisplayResourceKey => "Emulation.Family.Amiga";
    public IReadOnlyList<EmulationMachineDefinition> Machines => AmigaMachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(
        Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is not ["--amiga-core-host", var pipeName, var videoMapName]) return false;
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
                ["puae_model"] = model.BackendModel,
                ["puae_video_standard"] = "PAL",
                ["puae_floppy_multidrive"] = "disabled",
                ["puae_floppy_write_protection"] = "disabled"
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
        if (values.TryGetValue("puae_sound_volume_cd", out var cdVolume)
            && !string.IsNullOrWhiteSpace(cdVolume))
            options["puae_sound_volume_cd"] = cdVolume.TrimEnd('%') + "%";
        if (values.GetValueOrDefault(AmigaSettingsConstants.CpuSpeed)?.Split('|') is [var throttle, var multiplier])
        {
            options["puae_cpu_throttle"] = throttle;
            options["puae_cpu_multiplier"] = multiplier;
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
                values.GetValueOrDefault(AmigaSettingsConstants.ParallelJoystickAdapter) == "enabled"
        };
        return amiga with
        {
            Options = options,
            KickstartPath = values.GetValueOrDefault(AmigaSettingsConstants.KickstartPath) ?? string.Empty,
            ExtendedRomPath = values.GetValueOrDefault(AmigaSettingsConstants.ExtendedRomPath),
            RomKeyPath = values.GetValueOrDefault(AmigaSettingsConstants.RomKeyPath),
            AudioEnabled = values.GetValueOrDefault(AmigaSettingsConstants.AudioEnabled) == "enabled",
            Audio = currentAudio with
            {
                OutputDeviceId = string.IsNullOrWhiteSpace(output) ? null : output,
                LatencyMilliseconds = latency,
                Interpolation = options.GetValueOrDefault("puae_sound_interpol") ?? currentAudio.Interpolation,
                Filter = options.GetValueOrDefault("puae_sound_filter") ?? currentAudio.Filter,
                StereoSeparation = stereo
            },
            VideoRenderer = renderer,
            Input = input
        };
    }

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) =>
        AmigaConfigurationSummaryFunctions.Create(configuration as AmigaMachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) =>
        AmigaInputSettingsFunctions.Describe(configuration as AmigaMachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration,
        EmulationInputSettings settings) => AmigaInputSettingsFunctions.Apply(
        configuration as AmigaMachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

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
        return ValueTask.FromResult(new EmulationEmulatorInstallation("puae", version));
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
                FirmwareCompatibility(firmware, machineId))).ToArray();
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>(entries);
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration,
        EmulationFirmwareCandidate firmware)
    {
        var amiga = configuration as AmigaMachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        var inspected = AmigaFirmwareCatalog.Inspect(firmware.Path);
        return inspected.Type switch
        {
            AmigaFirmwareType.Kickstart => amiga with { KickstartPath = inspected.Path },
            AmigaFirmwareType.ExtendedRom => amiga with { ExtendedRomPath = inspected.Path },
            AmigaFirmwareType.RomKey => amiga with { RomKeyPath = inspected.Path },
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
            throw new FileNotFoundException("Kickstart", amiga.KickstartPath);
        var runtime = await AmigaRuntimeMediaFunctions.PrepareConfigurationAsync(amiga,
            services.ConvertedMediaDirectory).ConfigureAwait(false);
        var corePath = await new AmigaCoreProvider(_httpClient, _coreDirectory)
            .FindInstalledPathAsync(Path.Combine(AppContext.BaseDirectory, "Emulation", "puae_libretro.dll"),
                cancellationToken).ConfigureAwait(false)
            ?? throw new EmulationMessageException(new EmulationMessage(
                EmulationMessageCategory.Emulator, EmulationMessageCode.EmulatorNotInstalled,
                EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog,
                new EmulationEmulatorMessageContext("puae")));
        var audio = runtime.Audio ?? new AmigaAudioConfiguration();
        var creationContext = new AmigaMachineCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
            value => Path.Combine(services.StatesDirectory, value.Id.ToString("N"), "Saves"));
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
