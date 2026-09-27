using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Modules;

public sealed class AtariEmulationModule : IEmulationModule, IEmulationEmulatorManager,
    IEmulationFirmwareManager, IEmulationInputSettingsManager, IEmulationStorageSettingsManager,
    IEmulationModuleLocalization
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(AtariEmulationModule).Assembly, "GWGUI.Emulation.Atari.Resources.Emulation");

    public bool TryGetString(string key, System.Globalization.CultureInfo culture, out string value) =>
        Localization.TryGetString(key, culture, out value);

    private readonly ConfigurationStore _store;
    private readonly HttpClient _httpClient;
    private readonly string _coreDirectory;
    private readonly string _firmwareDirectory;
    private readonly Engine _engine = new();
    private IReadOnlyDictionary<string, CoreRelease> _availableReleases =
        new Dictionary<string, CoreRelease>(StringComparer.Ordinal);

    public AtariEmulationModule(string configurationDirectory, string pathBase, HttpClient httpClient,
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
    {
        exitCode = 0;
        if (arguments is [CoreHostConstants.CommandLineArgument, var pipeName, var videoMapName])
        {
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException();
            CoreHost.Run(pipeName, videoMapName);
            return true;
        }
        if (arguments is not [CoreOptionProbeConstants.CommandLineArgument, var corePath, var emulatorText]
            || !Enum.TryParse<Emulator>(emulatorText, out var emulator)) return false;
        try
        {
            Console.Out.WriteLine(CoreOptionProbe.Inspect(corePath, emulator).Count);
            exitCode = CoreOptionProbeConstants.SuccessExitCode;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(CoreOptionProbe.DescribeFailure(error));
            exitCode = CoreOptionProbeConstants.FailureExitCode;
        }
        return true;
    }

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var model = ModelCatalog.Parse(machineId);
        var compatibility = CompatibilityCatalog.Get(model);
        var visibleTabs = compatibility.VisibleTabs.ToHashSet();
        var tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
        {
            EmulationMachineTab.General => true,
            EmulationMachineTab.Cpu => visibleTabs.Contains(SettingsTab.Cpu),
            EmulationMachineTab.Ram => visibleTabs.Contains(SettingsTab.Memory),
            EmulationMachineTab.Rom => visibleTabs.Contains(SettingsTab.Firmware),
            EmulationMachineTab.Video => visibleTabs.Contains(SettingsTab.Video),
            EmulationMachineTab.Audio => visibleTabs.Contains(SettingsTab.Audio),
            EmulationMachineTab.Storage => visibleTabs.Contains(SettingsTab.Storage),
            EmulationMachineTab.Keyboard => visibleTabs.Contains(SettingsTab.Keyboard),
            EmulationMachineTab.Mouse => visibleTabs.Contains(SettingsTab.Mouse),
            EmulationMachineTab.Controllers => compatibility.ControllerPortCount > 0,
            _ => item.Value
        });
        var current = configuration as MachineConfiguration
            ?? (MachineConfiguration)CreateConfiguration(machineId);
        return new EmulationMachineSettings(machineId, new EmulationSettingsVisibility(tabs),
            SettingsDescriptionFunctions.Create(current), SettingsRules(model));
    }

    private static IReadOnlyList<EmulationSettingsRule> SettingsRules(MachineModel model)
    {
        if (!EightBitSettingsCatalog.SupportsOriginalComputerOptions(model)) return [];
        return
        [
            new(EmulationSettingsRuleCategory.MutuallyExclusive,
                EightBitSettingsConstants.MosaicMemoryOptionKey,
                EightBitSettingsConstants.AxlonMemoryOptionKey,
                EightBitSettingsConstants.Disabled),
            new(EmulationSettingsRuleCategory.VisibleWhenSourceDiffers,
                EightBitSettingsConstants.AxlonMemoryOptionKey,
                EightBitSettingsConstants.AxlonShadowOptionKey,
                EightBitSettingsConstants.Disabled)
        ];
    }

    public IEmulationConfiguration CreateConfiguration(string machineId) =>
        new MachineConfiguration(ModelCatalog.Parse(machineId));

    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId)
    {
        if (configuration is not MachineConfiguration)
            throw new ArgumentException(nameof(configuration));
        return new MachineConfiguration(ModelCatalog.Parse(machineId));
    }

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration,
        IReadOnlyDictionary<string, string?> values)
    {
        if (configuration is not MachineConfiguration atari)
            throw new ArgumentException(nameof(configuration));
        var options = new Dictionary<string, string>(atari.Options);
        foreach (var value in values)
        {
            if (value.Key is SettingsConstants.AudioEnabled
                or SettingsConstants.SystemFirmware or SettingsConstants.BasicFirmware
                or SettingsConstants.XegsFirmware or SettingsConstants.HardDiskFolder
                or SettingsConstants.CpuOriginalFrequency) continue;
            if (value.Value is null) options.Remove(value.Key);
            else options[value.Key] = value.Value;
        }
        IReadOnlyList<FirmwareConfiguration> firmwares = atari.Firmwares;
        foreach (var fieldId in new[]
                 {
                     SettingsConstants.SystemFirmware, SettingsConstants.BasicFirmware,
                     SettingsConstants.XegsFirmware
                 })
        {
            if (values.TryGetValue(fieldId, out var path))
                firmwares = ApplyFirmware(atari, firmwares, fieldId, path);
        }
        var folders = atari.Folders with
        {
            HardDisks = values.GetValueOrDefault(SettingsConstants.HardDiskFolder)
                ?? atari.Folders.HardDisks
        };
        return atari with
        {
            Firmwares = firmwares, Options = options, Folders = folders,
            AudioEnabled = values.GetValueOrDefault(SettingsConstants.AudioEnabled)
                == SettingsDescriptionFunctionsConstants.Enabled
        };
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration)
    {
        var atari = configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        var adapter = (MachineFactory)_engine.Adapter(atari);
        return adapter.PrepareOptions(adapter.GetConfiguredOptions(atari));
    }



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
        CancellationToken cancellationToken = default) => configuration is MachineConfiguration atari
        ? new ValueTask(_store.SaveAsync(atari, cancellationToken))
        : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) =>
        StorageSettingsFunctions.Describe(configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration,
        EmulationStorageSettings settings) => StorageSettingsFunctions.Apply(
        configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    private static IReadOnlyList<FirmwareConfiguration> ApplyFirmware(
        MachineConfiguration configuration, IReadOnlyList<FirmwareConfiguration> firmwares,
        string fieldId, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return FirmwareSelectionFunctions.ReplaceField(configuration.Model, firmwares, fieldId, null);
        var scanned = FirmwareScanFunctions.ScanFileAsync(path, configuration.Model, null,
            CancellationToken.None).GetAwaiter().GetResult();
        var selected = FirmwareScanFunctions.CreateSelection(scanned);
        if (!string.Equals(FirmwareSelectionFunctions.FieldId(configuration.Model, selected.Category),
                fieldId, StringComparison.Ordinal))
            throw new InvalidOperationException(nameof(path));
        return FirmwareSelectionFunctions.ReplaceField(configuration.Model, firmwares, fieldId, selected);
    }

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(
        CancellationToken cancellationToken = default) =>
        (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false))
        .Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is MachineConfiguration atari
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
        var emulator = CoreCatalog.Get(ModelCatalog.Parse(machineId)).Emulator;
        var installation = await new CoreReleaseService(_httpClient, _coreDirectory)
            .GetActiveInstallationAsync(emulator, cancellationToken).ConfigureAwait(false);
        var version = installation is null ? null : Path.GetFileName(installation.VersionDirectory);
        var core = CoreCatalog.Get(emulator);
        return new EmulationEmulatorInstallation(CoreCatalog.GetDefinition(core), version);
    }

    public async ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var atari = configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        return await GetInstallationAsync(CoreCatalog.Get(atari.Core), cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var atari = configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        var results = new List<EmulationEmulatorInstallation>();
        foreach (var core in CoreCatalog.GetAll(atari.Model))
            results.Add(await GetInstallationAsync(core, cancellationToken).ConfigureAwait(false));
        return results;
    }

    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration,
        string emulatorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var atari = configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        var core = CoreCatalog.Get(emulatorId);
        if (!core.Models.Contains(atari.Model))
            throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);
        return ValueTask.FromResult<IEmulationConfiguration>(atari with { Core = core.Emulator });
    }

    private async ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorCatalogEntry core, CancellationToken cancellationToken)
    {
        var installation = await new CoreReleaseService(_httpClient, _coreDirectory)
            .GetActiveInstallationAsync(core.Emulator, cancellationToken).ConfigureAwait(false);
        var version = installation is null ? null : Path.GetFileName(installation.VersionDirectory);
        return new EmulationEmulatorInstallation(CoreCatalog.GetDefinition(core), version);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId,
        CancellationToken cancellationToken = default)
    {
        var emulator = CoreCatalog.Get(ModelCatalog.Parse(machineId)).Emulator;
        var releases = await new CoreReleaseService(_httpClient, _coreDirectory)
            .GetAvailableAsync(emulator, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id,
            $"{item.DeclaredVersion} · {item.PublishedUtc.LocalDateTime:g}", item.DeclaredVersion)).ToArray();
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var atari = configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        return FindReleasesAsync(atari.Core, cancellationToken);
    }

    public async ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var emulator = CoreCatalog.Get(ModelCatalog.Parse(machineId)).Emulator;
        if (!_availableReleases.TryGetValue(release.Id, out var selected) || selected.Emulator != emulator)
            throw new ArgumentException(nameof(release));
        var adapter = progress is null ? null : new Progress<CoreInstallProgress>(value =>
            progress.Report(value.Fraction ?? 0));
        var installation = await new CoreReleaseService(_httpClient, _coreDirectory)
            .InstallAsync(selected, adapter, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    public ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration,
        EmulationEmulatorRelease release, IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var atari = configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        return InstallAsync(atari.Core, release, progress, cancellationToken);
    }

    private async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        Emulator emulator, CancellationToken cancellationToken)
    {
        var releases = await new CoreReleaseService(_httpClient, _coreDirectory)
            .GetAvailableAsync(emulator, cancellationToken).ConfigureAwait(false);
        _availableReleases = releases.ToDictionary(item => item.Id, StringComparer.Ordinal);
        return releases.Select(item => new EmulationEmulatorRelease(item.Id,
            $"{item.DeclaredVersion} · {item.PublishedUtc.LocalDateTime:g}", item.DeclaredVersion)).ToArray();
    }

    private async ValueTask<string> InstallAsync(Emulator emulator, EmulationEmulatorRelease release,
        IProgress<double>? progress, CancellationToken cancellationToken)
    {
        if (!_availableReleases.TryGetValue(release.Id, out var selected) || selected.Emulator != emulator)
            throw new ArgumentException(nameof(release));
        var adapter = progress is null ? null : new Progress<CoreInstallProgress>(value =>
            progress.Report(value.Fraction ?? 0));
        var installation = await new CoreReleaseService(_httpClient, _coreDirectory)
            .InstallAsync(selected, adapter, cancellationToken).ConfigureAwait(false);
        return installation.LibraryPath;
    }

    public string GetFirmwareDirectory(string machineId)
    {
        var family = StorageConfigurationFunctions.Family(ModelCatalog.Parse(machineId));
        return Path.Combine(_firmwareDirectory, FirmwareScanFunctions.FamilyDirectoryName(family));
    }

    public async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId,
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var atari = configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        var paths = FirmwareScanFunctions.EnumerateCandidates(_firmwareDirectory)
            .Where(path => string.Equals(Path.GetDirectoryName(path), GetFirmwareDirectory(machineId),
                StringComparison.OrdinalIgnoreCase));
        var entries = new List<EmulationFirmwareCandidate>();
        foreach (var path in paths)
        {
            var scanned = await FirmwareScanFunctions.ScanFileAsync(path, atari.Model, null,
                cancellationToken).ConfigureAwait(false);
            var name = scanned.Definition?.Category == FirmwareCategory.Tos
                ? $"{MachineConfigurationConstants.TosLabel} {scanned.Definition.Version}"
                : Path.GetFileName(scanned.Path);
            var compatibility = ToFirmwareCompatibility(scanned.Compatibility);
            entries.Add(new EmulationFirmwareCandidate(scanned.Md5 ?? scanned.Path, scanned.Path, name, null,
                compatibility, compatibility == EmulationFirmwareCompatibility.Incompatible ||
                               scanned.Definition?.Category is null
                    ? null
                    : FirmwareSelectionFunctions.FieldId(atari.Model, scanned.Definition.Category.Value)
                      ?? SettingsConstants.SystemFirmware));
        }
        return entries;
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration,
        EmulationFirmwareCandidate firmware)
    {
        var atari = configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration));
        if (firmware.DestinationFieldId is not (SettingsConstants.SystemFirmware
            or SettingsConstants.BasicFirmware or SettingsConstants.XegsFirmware))
            throw new InvalidOperationException(nameof(firmware));
        var scanned = FirmwareScanFunctions.ScanFileAsync(firmware.Path, atari.Model, null,
            CancellationToken.None).GetAwaiter().GetResult();
        var selected = FirmwareScanFunctions.CreateSelection(scanned);
        var actualFieldId = FirmwareSelectionFunctions.FieldId(atari.Model, selected.Category);
        var configured = actualFieldId is null
            ? atari.Firmwares.Where(item => item.Category != selected.Category).Append(selected).ToArray()
            : FirmwareSelectionFunctions.ReplaceField(atari.Model, atari.Firmwares,
                actualFieldId, selected);
        return atari with { Firmwares = configured };
    }

    private static EmulationFirmwareCompatibility ToFirmwareCompatibility(
        FirmwareCompatibility compatibility) => compatibility switch
        {
            FirmwareCompatibility.Compatible => EmulationFirmwareCompatibility.Compatible,
            FirmwareCompatibility.PartiallyCompatible => EmulationFirmwareCompatibility.PartiallyCompatible,
            _ => EmulationFirmwareCompatibility.Incompatible
        };

    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration,
        EmulationRuntimeServices services, CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration atari)
            throw new ArgumentException(nameof(configuration));
        var corePath = await new CoreProvider(_httpClient, _coreDirectory)
            .FindInstalledPathAsync(atari.Core, cancellationToken).ConfigureAwait(false)
            ?? throw new EmulationMessageException(new EmulationMessage(
                EmulationMessageCategory.Emulator, EmulationMessageCode.EmulatorNotInstalled,
                EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog,
                new EmulationEmulatorMessageContext(CoreCatalog.Get(atari.Core).Id)));
        var audioDevice = atari.Options.GetValueOrDefault(VideoAudioSettingsConstants.AudioOutputOption);
        if (string.Equals(audioDevice, VideoAudioSettingsConstants.DefaultAudioOutput,
                StringComparison.Ordinal)) audioDevice = null;
        var latency = atari.Options.TryGetValue(VideoAudioSettingsConstants.AudioLatencyOption,
                out var configuredLatency) && int.TryParse(configuredLatency, out var parsedLatency)
            ? parsedLatency : VideoAudioSettingsConstants.DefaultAudioLatencyMilliseconds;
        var creationContext = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audioDevice, latency),
            value => Path.Combine(services.StatesDirectory,
                value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)));
        var compatibility = CompatibilityCatalog.Get(atari.Model);
        var storage = StorageSettingsFunctions.Describe(atari);
        var devices = storage.AvailableDevices
            .Where(device => storage.ConfiguredSlots.Contains(device.Slot)).ToArray();
        var mounted = atari.Media.Select(EmulationMediaConversionFunctions.ToCommon)
            .OfType<EmulationMedia>().ToArray();
        return new EmulationMachineRuntime(atari,
            CreateMachineFactory(_engine, atari, creationContext), devices, mounted,
            MachineCatalog.All.First(model => model.Id == atari.MachineId).DisplayResourceKey,
            compatibility.VisibleTabs.Contains(SettingsTab.Mouse));
    }

    private static Func<IReadOnlyList<EmulationMedia>, IEmulatedMachine> CreateMachineFactory(
        Engine engine,
        MachineConfiguration configuration,
        EmulatorCreationContext context) =>
        media => engine.CreateMachine(WithMedia(configuration, media), context);

    private static MachineConfiguration WithMedia(MachineConfiguration configuration,
        IEnumerable<EmulationMedia> media)
    {
        var converted = media.Select(item => EmulationMediaConversionFunctions.ToAtari(item,
            configuration.Media)).ToArray();
        return configuration with { Media = converted };
    }

}
