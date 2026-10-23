using GWGUI.Emulation;

using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;
using AudioFunctions = GWGUI.Emulation.Snk.Common.Functions.AudioFunctions;
using System.Globalization;
using System.IO;
using System.Net.Http;
using GWGUI.Emulation.Snk.Modules;

namespace GWGUI.Emulation.Snk.Common.Services;

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
        return new SnkEmulationModule(Path.Combine(context.ModuleDirectory, EmulationModuleConstants.ConfigurationsDirectoryName), context.DataDirectory, context.HttpClient, Path.Combine(context.ModuleDirectory, EmulationModuleConstants.CoreDirectoryName));
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
        ConfigurationFunctions.Validate(configuration);
        return _adapters.TryGetValue(EmulatorCatalog.Get(configuration.Core).Id, out var adapter) ? adapter.Create(configuration, context) : throw new ArgumentOutOfRangeException(nameof(configuration));
    }

    /// <summary>Retrouve l’adaptateur sélectionné sans charger de cœur ; null conserve le parcours sans sélection.</summary>
    internal IEmulatorAdapter? SelectedAdapter(MachineConfiguration configuration) => _adapters.Values.FirstOrDefault(adapter => adapter.EmulatorKey == configuration.Core.ToString());

    internal IEmulatorAdapter Adapter(MachineConfiguration configuration) => _adapters[EmulatorCatalog.Get(configuration.Core).Id];
    internal IEmulatorAdapter Adapter(Emulator emulator) => Adapter(EmulatorCatalog.Get(emulator).Id);

    internal IEmulatorAdapter Adapter(string emulatorId) => _adapters.TryGetValue(emulatorId, out var adapter) ? adapter : throw new ArgumentOutOfRangeException(nameof(emulatorId));

    internal IReadOnlyList<IEmulatorAdapter> Adapters(string machineId) => _adapters.Values .Where(adapter => adapter.Definition.MachineIds.Contains(machineId)) .OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).ToArray();

    internal bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        foreach (var adapter in _adapters.Values) if (adapter.TryHandleHostCommand(arguments, out exitCode)) return true;
        exitCode = CoreHostConstants.NativeOperationSuccess;
        return false;
    }

    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(SnkEmulationModule).Assembly, EmulationModuleConstants.LocalizationResourceName);
    private readonly ConfigurationStore _store = null !;
    private readonly EmulatorManagementContext _managementContext = null !;
    private readonly string _firmwareDirectory = null !;
    /// <summary>Configure les opérations du module avec ses chemins et son client HTTP emprunté ; la construction sans paramètres conserve le parcours de catalogue.</summary>
    public Engine(string configurationDirectory, string pathBase, HttpClient httpClient, string coreDirectory) : this()
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _managementContext = new EmulatorManagementContext(httpClient, coreDirectory);
        _firmwareDirectory = Path.Combine(pathBase, EmulationPathConstants.RootDirectoryName, EmulationPathConstants.MachinesDirectoryName, EmulationModuleConstants.ModuleId, EmulationPathConstants.FirmwareDirectoryName);
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    // Brand image is independent of the machine photographs.
    public string BrandImageResourceName => $"{EmulationModuleConstants.AssetResourcePrefix}.snk.png";
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, tab => true));

    public bool TryGetString(string key, CultureInfo culture, out string value) => Localization.TryGetString(key, culture, out value);
    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var model = ModelCatalog.Parse(machineId);
        var current = configuration is null ? new MachineConfiguration(model) : Require(configuration);
        if (current.Model != model)
            throw new ArgumentException(nameof(configuration));
        var tabs = DefaultVisibility.Tabs.ToDictionary(
            item => item.Key,
            item => item.Key switch
        {
            EmulationMachineTab.Keyboard => false,
            EmulationMachineTab.Mouse => false,
            EmulationMachineTab.Rom => FirmwareFunctions.Roles(current.Model, current.Core).Count > 0,
            EmulationMachineTab.Cpu => true,
            EmulationMachineTab.Ram => HardwareModelCatalog.Get(current.Model).MainMemoryBytes is not null,
            EmulationMachineTab.Controllers => ConfigurationFunctions.ControllerPortCount(current) > BufferConstants.EmptyCollectionCount,
            EmulationMachineTab.Storage => true,
            _ => item.Value
        }
        );
        return new EmulationMachineSettings(machineId, new EmulationSettingsVisibility(tabs), this.SelectedAdapter(current)?.DescribeSettings(current) ?? SettingsDescriptionFunctions.Create(current));
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
        var editable = CommonCoreHostFunctions.OptionProcessing.EditableFieldIds(SettingsDescriptionFunctions.Create(current));
        var options = new Dictionary<string, string>(current.Options, StringComparer.Ordinal);
        CommonCoreHostFunctions.OptionProcessing.ApplyValues(options, values,
            optionKey => !editable.Contains(optionKey) || optionKey is SettingsConstants.AudioEnabled or SettingsConstants.InitialContent || FirmwareFunctions.IsField(optionKey));

        var result = current with
        {
            Firmwares = FirmwareFunctions.Apply(current, values),
            Options = options,
            InitialContent = editable.Contains(SettingsConstants.InitialContent) && values.TryGetValue(SettingsConstants.InitialContent, out var initialContent) ? string.IsNullOrWhiteSpace(initialContent) ? null : initialContent : current.InitialContent,
            AudioEnabled = values.TryGetValue(SettingsConstants.AudioEnabled, out var enabled) && editable.Contains(SettingsConstants.AudioEnabled) ? enabled == EmulationSettingConstants.Enabled : current.AudioEnabled
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration) => this.Adapter(Require(configuration)).GetRuntimeOptions(Require(configuration));
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

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default) => PreferredAdapter(machineId).GetInstallationAsync(_managementContext, cancellationToken);
    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => this.Adapter(Require(configuration)).GetInstallationAsync(_managementContext, cancellationToken);
    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => this.Adapter(Require(configuration)).FindReleasesAsync(_managementContext, cancellationToken);
    public ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration,
        EmulationEmulatorRelease release,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default) => this.Adapter(Require(configuration)).InstallAsync(_managementContext, release, progress, cancellationToken);
    public async ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var results = new List<EmulationEmulatorInstallation>();
        foreach (var adapter in this.Adapters(Require(configuration).MachineId))
            results.Add(await adapter.GetInstallationAsync(_managementContext, cancellationToken).ConfigureAwait(false));
        return results;
    }

    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current = Require(configuration);
        var adapter = this.Adapter(emulatorId);
        if (!adapter.Definition.MachineIds.Contains(current.MachineId))
            throw new ArgumentOutOfRangeException(nameof(emulatorId));
        var core = Enum.Parse<Emulator>(adapter.EmulatorKey);
        var options = CommonCoreHostFunctions.OptionProcessing.FilterForEmulator(CoreOptionValidationFunctions.ForCore(core).Select(option => option.Key), current.Options, SettingsConstants.AudioOutput, SettingsConstants.AudioLatency, VideoAudioSettingsConstants.AudioVolumeOption);
        var controllers = current.Input.Controllers?.Select(binding => InputSettingsFunctions.Peripherals(core).Contains(binding.Peripheral) ? binding : binding with { Peripheral = InputSettingsFunctions.DefaultPeripheral(current.Model) }).ToArray();
        var result = current with
        {
            Core = core,
            Options = options,
            Input = current.Input with
            {
                Controllers = controllers
            }
        };
        var devices = adapter is { } mediaAdapter ? mediaAdapter.DescribeMediaDevices(result) : [];
        var slots = devices.Select(device => device.Slot).ToHashSet();
        if (result.Media.Any(media => !slots.Contains(media.Slot)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(emulatorId));
        result = result with { ConfiguredSlots = result.ConfiguredSlots.Where(slots.Contains).ToArray() };
        ConfigurationFunctions.Validate(result);
        return ValueTask.FromResult<IEmulationConfiguration>(result);
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default) => PreferredAdapter(machineId).FindReleasesAsync(_managementContext, cancellationToken);
    public ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => PreferredAdapter(machineId).InstallAsync(_managementContext, release, progress, cancellationToken);
    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default)
    {
        var current = Require(configuration);
        var adapter = this.Adapter(current);
        var corePath = await adapter.FindInstalledCorePathAsync(_managementContext, cancellationToken).ConfigureAwait(false) ?? throw CommonCoreHostFunctions.Messages.EmulatorNotInstalled(adapter.EmulatorId);
        var output = AudioFunctions.Processing.ResolveRuntimeOutput(current.Options, SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput);
        var latency = AudioFunctions.Processing.ResolveRuntimeLatency(current.Options, SettingsConstants.AudioLatency);
        var context = new EmulatorCreationContext(
            services.SessionsDirectory,
            corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(output, latency),
            value => value.Folders.Saves ?? Path.Combine(services.StatesDirectory, EmulationModuleConstants.SavesDirectoryName, value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat))
        );
        var storage = StorageSettingsFunctions.Describe(current);
        return new EmulationMachineRuntime(
            current,
            media =>
        {
            var selected = current with
            {
                Media = media.Select(item => EmulationMediaConversionFunctions.ToModule(item, current.Media)).ToArray()
            };
            return this.CreateMachine(selected, context);
        },
            storage.AvailableDevices,
            storage.MountedMedia,
            ModelCatalog.All.Single(model => model.Id == current.MachineId).DisplayResourceKey,
            adapter.SupportsPointerCapture(current)
        );
    }

    private IEmulatorAdapter PreferredAdapter(string machineId) => this.Adapters(ModelCatalog.Parse(machineId).ToString()).FirstOrDefault(adapter => adapter.EmulatorKey
        == ConfigurationFunctions.DefaultEmulator(ModelCatalog.Parse(machineId)).ToString())
        ?? throw new ArgumentOutOfRangeException(nameof(machineId));
    public string GetFirmwareDirectory(string machineId)
    {
        _ = ModelCatalog.Parse(machineId);
        return _firmwareDirectory;
    }

    public async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var current = Require(configuration);
        if (current.Model != ModelCatalog.Parse(machineId))
            throw new ArgumentException(nameof(machineId));
        var directory = GetFirmwareDirectory(machineId);
        return await CommonCoreHostFunctions.FirmwareProcessing.ScanDirectoryAsync(directory,
            (path, token) => FirmwareFunctions.ScanFileAsync(current, path, token), cancellationToken).ConfigureAwait(false);
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware)
    {
        var current = Require(configuration);
        var scanned = FirmwareFunctions.ScanFileAsync(current, firmware.Path, CancellationToken.None).GetAwaiter().GetResult();
        var selected = FirmwareFunctions.Select(scanned, current.Model, current.Core);
        var result = current with
        {
            Firmwares = current.Firmwares.Where(item => item.Category != selected.Category).Append(selected).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }

    private static MachineConfiguration Require(IEmulationConfiguration configuration) => configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
}
