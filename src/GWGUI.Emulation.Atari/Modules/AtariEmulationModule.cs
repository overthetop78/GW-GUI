using GWGUI.Emulation;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Emulation.Atari;

public sealed class AtariEmulationModule : IEmulationModule, IEmulationEmulatorManager,
    IEmulationFirmwareManager, IEmulationInputSettingsManager, IEmulationStorageSettingsManager
{
    private readonly AtariConfigurationStore _store;
    private readonly HttpClient _httpClient;
    private readonly string _coreDirectory;
    private readonly string _firmwareDirectory;
    private readonly AtariEngine _engine = new();
    private IReadOnlyDictionary<string, AtariCoreRelease> _availableReleases =
        new Dictionary<string, AtariCoreRelease>(StringComparer.Ordinal);

    public AtariEmulationModule(string configurationDirectory, string pathBase, HttpClient httpClient,
        string coreDirectory)
    {
        _store = new AtariConfigurationStore(configurationDirectory, pathBase);
        _httpClient = httpClient;
        _coreDirectory = coreDirectory;
        _firmwareDirectory = Path.Combine(pathBase, "Emulation", AtariFirmwareConstants.FirmwareDirectoryName,
            "Atari");
    }

    public string Id => "atari";
    public string DisplayResourceKey => "Emulation.Family.Atari";
    public IReadOnlyList<EmulationMachineDefinition> Machines => AtariModelCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(
        Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        if (arguments is [AtariCoreHostConstants.CommandLineArgument, var pipeName, var videoMapName])
        {
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException();
            AtariCoreHost.Run(pipeName, videoMapName);
            return true;
        }
        if (arguments is not [AtariCoreOptionProbeConstants.CommandLineArgument, var corePath, var emulatorText]
            || !Enum.TryParse<AtariEmulator>(emulatorText, out var emulator)) return false;
        try
        {
            Console.Out.WriteLine(AtariCoreOptionProbe.Inspect(corePath, emulator).Count);
            exitCode = AtariCoreOptionProbeConstants.SuccessExitCode;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(AtariCoreOptionProbe.DescribeFailure(error));
            exitCode = AtariCoreOptionProbeConstants.FailureExitCode;
        }
        return true;
    }

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var model = AtariModelCatalog.Parse(machineId);
        var compatibility = AtariCompatibilityCatalog.Get(model);
        var visibleTabs = compatibility.VisibleTabs.ToHashSet();
        var tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
        {
            EmulationMachineTab.General => true,
            EmulationMachineTab.Cpu => visibleTabs.Contains(AtariSettingsTab.Cpu),
            EmulationMachineTab.Ram => visibleTabs.Contains(AtariSettingsTab.Memory),
            EmulationMachineTab.Rom => visibleTabs.Contains(AtariSettingsTab.Firmware),
            EmulationMachineTab.Video => visibleTabs.Contains(AtariSettingsTab.Video),
            EmulationMachineTab.Audio => visibleTabs.Contains(AtariSettingsTab.Audio),
            EmulationMachineTab.Storage => visibleTabs.Contains(AtariSettingsTab.Storage),
            EmulationMachineTab.Keyboard => visibleTabs.Contains(AtariSettingsTab.Keyboard),
            EmulationMachineTab.Mouse => visibleTabs.Contains(AtariSettingsTab.Mouse),
            EmulationMachineTab.Controllers => compatibility.ControllerPortCount > 0,
            _ => item.Value
        });
        var current = configuration as AtariMachineConfiguration
            ?? (AtariMachineConfiguration)CreateConfiguration(machineId);
        return new EmulationMachineSettings(machineId, new EmulationSettingsVisibility(tabs),
            AtariSettingsDescriptionFunctions.Create(current), SettingsRules(model));
    }

    private static IReadOnlyList<EmulationSettingsRule> SettingsRules(AtariMachineModel model)
    {
        if (!AtariEightBitSettingsCatalog.SupportsOriginalComputerOptions(model)) return [];
        return
        [
            new(EmulationSettingsRuleCategory.MutuallyExclusive,
                AtariEightBitSettingsConstants.MosaicMemoryOptionKey,
                AtariEightBitSettingsConstants.AxlonMemoryOptionKey,
                AtariEightBitSettingsConstants.Disabled),
            new(EmulationSettingsRuleCategory.VisibleWhenSourceDiffers,
                AtariEightBitSettingsConstants.AxlonMemoryOptionKey,
                AtariEightBitSettingsConstants.AxlonShadowOptionKey,
                AtariEightBitSettingsConstants.Disabled)
        ];
    }

    public IEmulationConfiguration CreateConfiguration(string machineId) =>
        new AtariMachineConfiguration(AtariModelCatalog.Parse(machineId));

    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId)
    {
        if (configuration is not AtariMachineConfiguration)
            throw new ArgumentException(nameof(configuration));
        return new AtariMachineConfiguration(AtariModelCatalog.Parse(machineId));
    }

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration,
        IReadOnlyDictionary<string, string?> values)
    {
        if (configuration is not AtariMachineConfiguration atari)
            throw new ArgumentException(nameof(configuration));
        var options = new Dictionary<string, string>(atari.Options);
        foreach (var value in values)
        {
            if (value.Key is AtariSettingsConstants.AudioEnabled or AtariSettingsConstants.VideoRenderer
                or AtariSettingsConstants.SystemFirmware or AtariSettingsConstants.HardDiskFolder
                or AtariSettingsConstants.CpuOriginalFrequency) continue;
            if (value.Value is null) options.Remove(value.Key);
            else options[value.Key] = value.Value;
        }
        var renderer = values.TryGetValue(AtariSettingsConstants.VideoRenderer, out var rendererValue)
            && Enum.TryParse<EmulationVideoRenderer>(rendererValue, out var selectedRenderer)
                ? selectedRenderer : atari.VideoRenderer;
        var firmwares = ApplySystemFirmware(atari, values.GetValueOrDefault(AtariSettingsConstants.SystemFirmware));
        var folders = atari.Folders with
        {
            HardDisks = values.GetValueOrDefault(AtariSettingsConstants.HardDiskFolder)
                ?? atari.Folders.HardDisks
        };
        return new AtariMachineConfiguration(atari.Model, firmwares, atari.Media, options, atari.Input,
            atari.Id, atari.SchemaVersion,
            values.GetValueOrDefault(AtariSettingsConstants.AudioEnabled) == "enabled",
            renderer, folders);
    }

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) =>
        AtariConfigurationSummaryFunctions.Create(configuration as AtariMachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) =>
        AtariInputSettingsFunctions.Describe(configuration as AtariMachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration,
        EmulationInputSettings settings) => AtariInputSettingsFunctions.Apply(
        configuration as AtariMachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) =>
        AtariStorageSettingsFunctions.Describe(configuration as AtariMachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration,
        EmulationStorageSettings settings) => AtariStorageSettingsFunctions.Apply(
        configuration as AtariMachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    private static IReadOnlyList<AtariFirmwareConfiguration> ApplySystemFirmware(
        AtariMachineConfiguration configuration, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            var category = AtariCompatibilityCatalog.Get(configuration.Model).Core == AtariEmulator.Hatari
                ? AtariFirmwareCategory.Tos : AtariFirmwareCategory.AtariSystemOs;
            return configuration.Firmwares.Where(item => item.Category != category).ToArray();
        }
        var scanned = AtariFirmwareScanFunctions.ScanFileAsync(path, configuration.Model, null,
            CancellationToken.None).GetAwaiter().GetResult();
        var selected = AtariFirmwareScanFunctions.CreateSelection(scanned);
        return configuration.Firmwares.Where(item => item.Category != selected.Category)
            .Append(selected).ToArray();
    }

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(
        CancellationToken cancellationToken = default) =>
        (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false))
        .Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is AtariMachineConfiguration atari
        ? new ValueTask(_store.SaveAsync(atari, cancellationToken))
        : ValueTask.FromException(new ArgumentException(nameof(configuration)));

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
        var emulator = AtariCoreCatalog.Get(AtariModelCatalog.Parse(machineId)).Emulator;
        var installation = await new AtariCoreReleaseService(_httpClient, _coreDirectory)
            .GetActiveInstallationAsync(emulator, cancellationToken).ConfigureAwait(false);
        var version = installation is null ? null : Path.GetFileName(installation.VersionDirectory);
        return new EmulationEmulatorInstallation(AtariCoreCatalog.Get(emulator).Id, version);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId,
        CancellationToken cancellationToken = default)
    {
        var emulator = AtariCoreCatalog.Get(AtariModelCatalog.Parse(machineId)).Emulator;
        var releases = await new AtariCoreReleaseService(_httpClient, _coreDirectory)
            .GetAvailableAsync(emulator, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id,
            $"{item.DeclaredVersion} · {item.PublishedUtc.LocalDateTime:g}", item.DeclaredVersion)).ToArray();
    }

    public async ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var emulator = AtariCoreCatalog.Get(AtariModelCatalog.Parse(machineId)).Emulator;
        if (!_availableReleases.TryGetValue(release.Id, out var selected) || selected.Emulator != emulator)
            throw new ArgumentException(nameof(release));
        var adapter = progress is null ? null : new Progress<AtariCoreInstallProgress>(value =>
            progress.Report(value.Fraction ?? 0));
        var installation = await new AtariCoreReleaseService(_httpClient, _coreDirectory)
            .InstallAsync(selected, adapter, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    public string GetFirmwareDirectory(string machineId)
    {
        var family = AtariStorageConfigurationFunctions.Family(AtariModelCatalog.Parse(machineId));
        return Path.Combine(_firmwareDirectory, AtariFirmwareScanFunctions.FamilyDirectoryName(family));
    }

    public async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId,
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var atari = configuration as AtariMachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        var paths = AtariFirmwareScanFunctions.EnumerateCandidates(_firmwareDirectory)
            .Where(path => string.Equals(Path.GetDirectoryName(path), GetFirmwareDirectory(machineId),
                StringComparison.OrdinalIgnoreCase));
        var entries = new List<EmulationFirmwareCandidate>();
        foreach (var path in paths)
        {
            var scanned = await AtariFirmwareScanFunctions.ScanFileAsync(path, atari.Model, null,
                cancellationToken).ConfigureAwait(false);
            var name = scanned.Definition?.Version ?? Path.GetFileName(scanned.Path);
            entries.Add(new EmulationFirmwareCandidate(scanned.Md5 ?? scanned.Path, scanned.Path, name, null,
                ToFirmwareCompatibility(scanned.Compatibility)));
        }
        return entries;
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration,
        EmulationFirmwareCandidate firmware)
    {
        var atari = configuration as AtariMachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        var scanned = AtariFirmwareScanFunctions.ScanFileAsync(firmware.Path, atari.Model, null,
            CancellationToken.None).GetAwaiter().GetResult();
        var selected = AtariFirmwareScanFunctions.CreateSelection(scanned);
        var configured = atari.Firmwares.Where(item => item.Category != selected.Category).Append(selected).ToArray();
        return new AtariMachineConfiguration(atari.Model, configured, atari.Media, atari.Options, atari.Input,
            atari.Id, atari.SchemaVersion, atari.AudioEnabled, atari.VideoRenderer, atari.Folders);
    }

    private static EmulationFirmwareCompatibility ToFirmwareCompatibility(
        AtariFirmwareCompatibility compatibility) => compatibility switch
        {
            AtariFirmwareCompatibility.Compatible => EmulationFirmwareCompatibility.Compatible,
            AtariFirmwareCompatibility.PartiallyCompatible => EmulationFirmwareCompatibility.PartiallyCompatible,
            _ => EmulationFirmwareCompatibility.Incompatible
        };

    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration,
        EmulationRuntimeServices services, CancellationToken cancellationToken = default)
    {
        if (configuration is not AtariMachineConfiguration atari)
            throw new ArgumentException(nameof(configuration));
        var corePath = await new AtariCoreProvider(_httpClient, _coreDirectory)
            .FindInstalledPathAsync(atari.Core, cancellationToken).ConfigureAwait(false)
            ?? throw new EmulationMessageException(new EmulationMessage(
                EmulationMessageCategory.Emulator, EmulationMessageCode.EmulatorNotInstalled,
                EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog,
                new EmulationEmulatorMessageContext(AtariCoreCatalog.Get(atari.Core).Id)));
        var audioDevice = atari.Options.GetValueOrDefault(AtariConfigurationOptionConstants.AudioOutput);
        if (string.Equals(audioDevice, AtariConfigurationOptionConstants.DefaultAudioOutput,
                StringComparison.Ordinal)) audioDevice = null;
        var latency = atari.Options.TryGetValue(AtariConfigurationOptionConstants.AudioLatency,
                out var configuredLatency) && int.TryParse(configuredLatency, out var parsedLatency)
            ? parsedLatency : AtariConfigurationOptionConstants.DefaultAudioLatencyMilliseconds;
        var creationContext = new AtariMachineCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audioDevice, latency),
            value => Path.Combine(services.StatesDirectory, value.Id.ToString("N")));
        var compatibility = AtariCompatibilityCatalog.Get(atari.Model);
        var storage = AtariStorageSettingsFunctions.Describe(atari);
        var devices = storage.AvailableDevices
            .Where(device => storage.ConfiguredSlots.Contains(device.Slot)).ToArray();
        var mounted = atari.Media.Select(EmulationMediaConversionFunctions.ToCommon)
            .OfType<EmulationMedia>().ToArray();
        return new EmulationMachineRuntime(atari,
            CreateMachineFactory(_engine, atari, creationContext), devices, mounted,
            AtariModelCatalog.All.First(model => model.Id == atari.MachineId).DisplayResourceKey,
            compatibility.VisibleTabs.Contains(AtariSettingsTab.Mouse));
    }

    private static Func<IReadOnlyList<EmulationMedia>, IEmulatedMachine> CreateMachineFactory(
        AtariEngine engine,
        AtariMachineConfiguration configuration,
        AtariMachineCreationContext context) =>
        media => engine.CreateMachine(WithMedia(configuration, media), context);

    private static AtariMachineConfiguration WithMedia(AtariMachineConfiguration configuration,
        IEnumerable<EmulationMedia> media)
    {
        var converted = media.Select(item => EmulationMediaConversionFunctions.ToAtari(item,
            configuration.Media)).ToArray();
        return new AtariMachineConfiguration(configuration.Model, configuration.Firmwares, converted,
            configuration.Options, configuration.Input, configuration.Id, configuration.SchemaVersion,
            configuration.AudioEnabled, configuration.VideoRenderer, configuration.Folders);
    }

}
