using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Commodore.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Security.Cryptography;
using GWGUI.Emulation.Commodore.Modules;

namespace GWGUI.Emulation.Commodore.Common.Services;

internal sealed class Engine
{
    /// <summary>Construit le module depuis le contexte fourni par l’hôte.</summary>
    /// <param name="context">Racine des données, dossier du module et client HTTP emprunté.</param>
    /// <returns>Façade utilisant les services Common, prête à décrire les machines sans démarrer de cœur.</returns>
    /// <remarks>La construction conserve les emplacements de configuration et de firmware ainsi que
    /// leur ordre de préparation. Le client HTTP reste sous la responsabilité de son propriétaire.
    /// La machine et les ressources du cœur sont créées séparément lors du lancement du runtime.</remarks>
    public static IEmulationModule Create(EmulationModuleContext context) {
        ArgumentNullException.ThrowIfNull(context);
        var moduleDirectory = context.ModuleDirectory;
        Directory.CreateDirectory(Path.Combine(moduleDirectory, EmulationPathConstants.FirmwareDirectoryName));
        return new CommodoreEmulationModule( Path.Combine(moduleDirectory, CoreDirectoryConstants.ConfigurationsDirectoryName), context.DataDirectory, context.HttpClient, Path.Combine(moduleDirectory, CoreDirectoryConstants.CoreDirectoryName));
    }

    private readonly IReadOnlyDictionary<string, IEmulatorAdapter> _adapters;

    public Engine()
    {
        var adapters = EmulatorCatalog.CreateAdapters();
        _adapters = adapters.ToDictionary(adapter => adapter.EmulatorId, StringComparer.Ordinal);
    }

    internal IEmulatedMachine CreateMachine(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(context);
        return Adapter(configuration).Create(configuration, context);
    }

    /// <summary>Résout le backend choisi dans le catalogue possédé par cette instance Common.</summary>
    /// <param name="configuration">Configuration empruntée portant la sélection du backend.</param>
    /// <returns>La même instance du catalogue, ou null lorsque la sélection ne correspond à aucun backend.</returns>
    internal IEmulatorAdapter? SelectedAdapter(MachineConfiguration configuration) => _adapters.Values.FirstOrDefault(candidate => candidate.EmulatorKey == configuration.Core.ToString());

    internal IEmulatorAdapter Adapter(MachineConfiguration configuration) => _adapters[EmulatorCatalog.Get(configuration.Core).Id];

    internal IEmulatorAdapter Adapter(string emulatorId) => _adapters.TryGetValue(emulatorId, out var adapter) ? adapter : throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);

    internal bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        foreach (var adapter in _adapters.Values)
            if (adapter.TryHandleHostCommand(arguments, out exitCode)) return true;
        exitCode = CoreHostConstants.SuccessfulExitCode;
        return false;
    }

    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(CommodoreEmulationModule).Assembly, EmulationModuleConstants.LocalizationResourceName);

    public bool TryGetString(string key, System.Globalization.CultureInfo culture, out string value) => Localization.TryGetString(key, culture, out value);
    private readonly ConfigurationStore _store = null !;
    private readonly HttpClient _httpClient = null !;
    private readonly string _coreDirectory = null !;
    private readonly string _firmwareDirectory = null !;
    private EmulatorManagementContext EmulatorManagement(IEmulatorAdapter adapter) => new(_httpClient, Path.Combine(_coreDirectory, adapter.EmulatorId));
    /// <summary>Configure les opérations du module avec ses chemins et son client HTTP emprunté ; la construction sans paramètres conserve le parcours de catalogue.</summary>
    public Engine(string configurationDirectory, string pathBase, HttpClient httpClient, string coreDirectory) : this()
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _httpClient = httpClient;
        _coreDirectory = coreDirectory;
        _firmwareDirectory = Path.Combine(pathBase, EmulationPathConstants.RootDirectoryName, EmulationPathConstants.MachinesDirectoryName, FirmwareConstants.DirectoryName, EmulationPathConstants.FirmwareDirectoryName);
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    public string BrandImageResourceName => EmulationModuleConstants.BrandImageResourceName;
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var model = ModelCatalog.Get(machineId);
        var current = configuration as MachineConfiguration ?? (MachineConfiguration)CreateConfiguration(machineId);
        var visibility = DefaultVisibility with
        {
            Tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
            {
                EmulationMachineTab.Mouse => model.MouseButtonCount > MachineSettingsConstants.NoDevices,
                EmulationMachineTab.Controllers => model.ControllerPortCount > MachineSettingsConstants.NoDevices,
                EmulationMachineTab.Keyboard => model.HasKeyboard,
                EmulationMachineTab.Rom => this.Adapter(current).GetFirmwareSlots(current).Count > BufferConstants.EmptyCollectionCount,
                _ => item.Value
            })
        };
        return new EmulationMachineSettings(machineId, visibility, SettingsDescriptionFunctions.Create(model, current));
    }

    public IEmulationConfiguration CreateConfiguration(string machineId) => new MachineConfiguration(ModelCatalog.Get(machineId).Id, EmulatorCatalog.DefaultFor(machineId), Id: Guid.NewGuid());

    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId)
    {
        if (configuration is not MachineConfiguration) throw new ArgumentException(nameof(configuration));
        return CreateConfiguration(machineId);
    }

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        if (configuration is not MachineConfiguration current)
            throw new ArgumentException(nameof(configuration));
        var slots = this.Adapter(current).GetFirmwareSlots(current);
        var firmwareFields = slots.Select(slot => slot.FieldId).ToHashSet(StringComparer.Ordinal);
        var options = new Dictionary<string, string>(current.Options ?? new Dictionary<string, string>());
        CommonCoreHostFunctions.OptionProcessing.ApplyValues(options, values,
            optionKey => firmwareFields.Contains(optionKey) || optionKey is StorageDirectorySettings.HardDiskDirectory or MachineSettingsConstants.AudioEnabled
                or MachineSettingsConstants.AudioOutput
                or MachineSettingsConstants.AudioLatency or MachineSettingsConstants.AudioStereoSeparation);
        var currentAudio = current.Audio ?? new AudioConfiguration();
        var hasOutput = values.TryGetValue(MachineSettingsConstants.AudioOutput, out var output);
        var latency = int.TryParse(values.GetValueOrDefault(MachineSettingsConstants.AudioLatency), out var latencyValue) ? latencyValue : currentAudio.LatencyMilliseconds;
        var stereo = int.TryParse(values.GetValueOrDefault(MachineSettingsConstants.AudioStereoSeparation), out var stereoValue) ? stereoValue : currentAudio.StereoSeparation;
        var updated = current with
        {
            Options = options,
            HardDiskDirectory = values.TryGetValue(StorageDirectorySettings.HardDiskDirectory, out var directory) ? string.IsNullOrWhiteSpace(directory) ? null : directory : current.HardDiskDirectory,
            FirmwarePaths = ApplyFirmwarePaths(current, values, firmwareFields),
            AudioEnabled = values.TryGetValue(MachineSettingsConstants.AudioEnabled, out var audioEnabled) ? audioEnabled == MachineSettingsConstants.Enabled : current.AudioEnabled,
            Audio = currentAudio with
            {
                OutputDeviceId = hasOutput ? string.IsNullOrWhiteSpace(output) ? null : output : currentAudio.OutputDeviceId,
                LatencyMilliseconds = latency,
                StereoSeparation = stereo
            },
        };
        return this.Adapter(updated).ApplySettings(updated, values);
    }


    private static IReadOnlyDictionary<string, string> ApplyFirmwarePaths(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values, IReadOnlySet<string> fields)
    {
        var paths = new Dictionary<string, string>(configuration.FirmwarePaths ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var field in fields)
        {
            if (!values.TryGetValue(field, out var path)) continue;
            if (string.IsNullOrWhiteSpace(path)) paths.Remove(field);
            else paths[field] = path;
        }
        return paths;
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var adapter = SelectedAdapter(current);
        return adapter?.GetRuntimeOptions(current) ?? new Dictionary<string, string>(current.Options ?? new Dictionary<string, string>());
    }

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) => ConfigurationSummaryFunctions.Create(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)));

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => InputSettingsFunctions.Describe(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => InputSettingsFunctions.Apply(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is MachineConfiguration current ? new ValueTask(_store.SaveAsync(current, cancellationToken)) : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) => StorageSettingsFunctions.Describe(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings) => StorageSettingsFunctions.Apply(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false)).Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration current)
            return ValueTask.FromException(new ArgumentException(nameof(configuration)));
        ConfigurationValidationFunctions.ValidateForSave(current);
        return new ValueTask(_store.SaveAsync(current, cancellationToken));
    }

    public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }

    public async ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var definition = EmulatorCatalog.Get(EmulatorCatalog.DefaultFor(machineId));
        var adapter = this.Adapter(definition.Id);
        return await adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var definition = EmulatorCatalog.Get(EmulatorCatalog.DefaultFor(machineId));
        var adapter = this.Adapter(definition.Id);
        return await adapter.FindReleasesAsync(EmulatorManagement(adapter), cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var definition = EmulatorCatalog.Get(EmulatorCatalog.DefaultFor(machineId));
        var adapter = this.Adapter(definition.Id);
        return await adapter.InstallAsync(EmulatorManagement(adapter), release, progress, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var adapter = this.Adapter(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)));
        return await adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var results = new List<EmulationEmulatorInstallation>();
        foreach (var definition in EmulatorCatalog.GetAll(current.Model))
        {
            var adapter = this.Adapter(definition.Id);
            results.Add(await adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken).ConfigureAwait(false));
        }
        return results;
    }

    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var adapter = this.Adapter(emulatorId);
        if (!adapter.Definition.MachineIds.Contains(current.Model))
            throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);
        var selected = current with { Core = Enum.Parse<Emulator>(adapter.EmulatorKey), Options = null };
        var slots = adapter.GetFirmwareSlots(selected).Select(slot => slot.FieldId).ToHashSet(StringComparer.Ordinal);
        return ValueTask.FromResult<IEmulationConfiguration>(selected with
        {
            FirmwarePaths = current.FirmwarePaths?.Where(pair => slots.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
        });
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var adapter = this.Adapter(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)));
        return await adapter.FindReleasesAsync(EmulatorManagement(adapter), cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var adapter = this.Adapter(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)));
        return await adapter.InstallAsync(EmulatorManagement(adapter), release, progress, cancellationToken).ConfigureAwait(false);
    }

    public string GetFirmwareDirectory(string machineId)
    {
        _ = ModelCatalog.Get(machineId);
        return _firmwareDirectory;
    }

    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var current = (MachineConfiguration)configuration;
        var slots = this.Adapter(current).GetFirmwareSlots(current);
        if (slots.Count == MachineSettingsConstants.NoDevices)
            return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>([]);
        var directory = GetFirmwareDirectory(machineId);
        Directory.CreateDirectory(directory);
        var entries = this.Adapter(current).ScanFirmware(current, directory, cancellationToken);
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>(entries);
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        if (firmware.DestinationFieldId is not { } field
            || !this.Adapter(current).GetFirmwareSlots(current).Any(slot => slot.FieldId == field))
            throw new ArgumentException(nameof(firmware));
        return current.WithFirmwarePath(field, firmware.Path);
    }

    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration current)
            throw new ArgumentException(nameof(configuration));
        ConfigurationValidationFunctions.ValidateForSave(current);
        foreach (var slot in this.Adapter(current).GetFirmwareSlots(current))
            if (slot.IsRequired && string.IsNullOrWhiteSpace(current.FirmwarePath(slot.FieldId)))
                throw CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.Firmware, EmulationMessageCode.FirmwareMissing, null);
        var runtime = await RuntimeMediaFunctions.PrepareConfigurationAsync(current, services.ConvertedMediaDirectory).ConfigureAwait(false);
        var emulator = this.Adapter(runtime);
        var corePath = await emulator.FindInstalledCorePathAsync(EmulatorManagement(emulator), cancellationToken).ConfigureAwait(false)
            ?? throw CommonCoreHostFunctions.Messages.EmulatorNotInstalled(emulator.EmulatorId);
        var audio = runtime.Audio ?? new AudioConfiguration();
        var creationContext = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
            value => Path.Combine(services.StatesDirectory, value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat), CoreDirectoryConstants.SavesDirectoryName));
        var storage = StorageSettingsFunctions.Describe(runtime);
        var devices = storage.AvailableDevices.Where(device => storage.ConfiguredSlots.Contains(device.Slot)).ToArray();
        var mounted = emulator.ResolveConfiguredMedia(runtime);
        return new EmulationMachineRuntime(runtime,
            CreateMachineFactory(this, runtime, creationContext), devices, mounted,
            MachineCatalog.All.First(machine => machine.Id == runtime.Model).DisplayResourceKey, emulator.SupportsPointerCapture(runtime),
            (media, _) => RuntimeMediaFunctions.PrepareMediaAsync(runtime, media, services.ConvertedMediaDirectory));
    }

    private static Func<IReadOnlyList<EmulationMedia>, IEmulatedMachine> CreateMachineFactory(Engine engine, MachineConfiguration configuration, EmulatorCreationContext context) =>
        media => engine.CreateMachine(configuration with { Media = EmulationMediaConversionFunctions.FromCommon(media) }, context);
}
