using GWGUI.Emulation.Wenquxing.Common.Dictionaries;

using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Wenquxing.Common.Functions.CoreHostFunctions;
using AudioFunctions = GWGUI.Emulation.Wenquxing.Common.Functions.AudioFunctions;
using GWGUI.Emulation;
using System.Globalization;
using System.IO;
using System.Net.Http;
using GWGUI.Emulation.Wenquxing.Modules;

namespace GWGUI.Emulation.Wenquxing.Common.Services;

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
        return new WenquxingEmulationModule(Path.Combine(context.ModuleDirectory, EmulationModuleConstants.ConfigurationsDirectoryName), context.DataDirectory, context.HttpClient, Path.Combine(context.ModuleDirectory, EmulationModuleConstants.CoreDirectoryName));
    }

    /// <summary>Conserve la construction du catalogue sans services de persistance configurés.</summary>
    public Engine() { }

    private readonly IReadOnlyDictionary<string, IEmulatorAdapter> _adapters = EmulatorCatalog.CreateAdapters().ToDictionary(adapter => adapter.EmulatorId, StringComparer.Ordinal);
    internal IEmulatedMachine CreateMachine(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ConfigurationFunctions.Validate(configuration);
        return Adapter(configuration).Create(configuration, context);
    }
    /// <summary>Retrouve l’adaptateur sélectionné sans charger de cœur ; null conserve le parcours sans sélection.</summary>
    internal IEmulatorAdapter? SelectedAdapter(MachineConfiguration configuration) => _adapters.Values.FirstOrDefault(adapter => adapter.EmulatorKey == configuration.Core.ToString());

    internal IEmulatorAdapter Adapter(MachineConfiguration configuration) => Adapter(configuration.Core);
    internal IEmulatorAdapter Adapter(Emulator emulator) => _adapters.Values.SingleOrDefault(adapter => string.Equals(adapter.EmulatorKey, emulator.ToString(), StringComparison.Ordinal)) ?? throw new ArgumentOutOfRangeException(nameof(emulator));
    internal IEmulatorAdapter Adapter(string emulatorId) => _adapters.TryGetValue(emulatorId, out var adapter) ? adapter : throw new ArgumentOutOfRangeException(nameof(emulatorId));
    internal IReadOnlyList<IEmulatorAdapter> Adapters(string machineId) => _adapters.Values.Where(adapter => adapter.Definition.MachineIds.Contains(machineId)).OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).ToArray();
    internal bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        foreach (var adapter in _adapters.Values) if (adapter.TryHandleHostCommand(arguments, out exitCode)) return true;
        exitCode = CoreHostConstants.NativeOperationSuccess;
        return false;
    }

    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(WenquxingEmulationModule).Assembly, EmulationModuleConstants.LocalizationResourceName);
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
    public string BrandImageResourceName => $"{EmulationModuleConstants.AssetResourcePrefix}.wenquxing.png";
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(
            tab => tab,
            tab => tab is EmulationMachineTab.General or EmulationMachineTab.Cpu or EmulationMachineTab.Ram or EmulationMachineTab.Rom or EmulationMachineTab.Video or EmulationMachineTab.Audio or EmulationMachineTab.Keyboard
        ));

    public bool TryGetString(string key, CultureInfo culture, out string value) => Localization.TryGetString(key, culture, out value);
    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var model = ModelCatalog.Parse(machineId);
        var current = configuration is null ? new MachineConfiguration(model) : Require(configuration);
        if (current.Model != model)
            throw new ArgumentException(nameof(configuration));
        EmulationMachineSettings result = new EmulationMachineSettings(machineId, DefaultVisibility, this.SelectedAdapter(current)?.DescribeSettings(current) ?? SettingsDescriptionFunctions.Create(current));
        if (SelectedAdapter(current)?.SupportsVideoSettings == false && !result.Blocks.Any(block => block.Tab == EmulationMachineTab.Video))
        {
            var adjustedTabs = result.Visibility.Tabs.ToDictionary(pair => pair.Key, pair => pair.Value);
            adjustedTabs[EmulationMachineTab.Video] = false;
            result = result with { Visibility = new EmulationSettingsVisibility(adjustedTabs) };
        }
        return result;
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
            optionKey => !editable.Contains(optionKey) || optionKey == SettingsConstants.AudioEnabled || FirmwareFunctions.Requirements(current).Any(requirement => requirement.FieldId == optionKey));

        var result = current with
        {
            Options = options,
            Firmware = FirmwareFunctions.Apply(current, values),
            AudioEnabled = values.TryGetValue(SettingsConstants.AudioEnabled, out var enabled) && editable.Contains(SettingsConstants.AudioEnabled) ? enabled == EmulationSettingConstants.Enabled : current.AudioEnabled
        };
        result = SelectedAdapter(result)?.ApplySettings(result, values) ?? result;
        ConfigurationFunctions.Validate(result);
        return result;
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration)
    {
        var current = Require(configuration);
        return current.Core == Emulator.None ? current.Options : this.Adapter(current).GetRuntimeOptions(current);
    }

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) => ConfigurationSummaryFunctions.Create(Require(configuration));
    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => InputSettingsFunctions.Describe(Require(configuration));
    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => InputSettingsFunctions.Apply(Require(configuration), settings);
    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => SaveConfigurationAsync(configuration, cancellationToken);
    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false)).Cast<IEmulationConfiguration>().ToArray();
    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => new(_store.SaveAsync(Require(configuration), cancellationToken));
    public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default) => PreferredAdapter(machineId).GetInstallationAsync(_managementContext, cancellationToken);
    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => SelectedOrDefault(Require(configuration)).GetInstallationAsync(_managementContext, cancellationToken);
    public async ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var result = new List<EmulationEmulatorInstallation>();
        foreach (var adapter in this.Adapters(Require(configuration).MachineId))
            result.Add(await adapter.GetInstallationAsync(_managementContext, cancellationToken).ConfigureAwait(false));
        return result;
    }

    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current = Require(configuration);
        var adapter = this.Adapter(emulatorId);
        if (!adapter.Definition.MachineIds.Contains(current.MachineId))
            throw new ArgumentOutOfRangeException(nameof(emulatorId));
        var result = adapter.SelectConfiguration(current with
        {
            Core = Enum.Parse<Emulator>(adapter.EmulatorKey)
        });
        ConfigurationFunctions.Validate(result);
        return ValueTask.FromResult<IEmulationConfiguration>(result);
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default) => PreferredAdapter(machineId).FindReleasesAsync(_managementContext, cancellationToken);
    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => SelectedOrDefault(Require(configuration)).FindReleasesAsync(_managementContext, cancellationToken);
    public ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => PreferredAdapter(machineId).InstallAsync(_managementContext, release, progress, cancellationToken);
    public ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration,
        EmulationEmulatorRelease release,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default) => SelectedOrDefault(Require(configuration)).InstallAsync(_managementContext, release, progress, cancellationToken);
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
            value => Path.Combine(services.StatesDirectory, EmulationModuleConstants.SavesDirectoryName, value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat))
        );
        return new EmulationMachineRuntime(
            current,
            media => media.Count == 0 ? this.CreateMachine(current, context) : throw CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.Media, EmulationMessageCode.MediaUnsupported, null),
            [],
            [],
            ModelCatalog.All.Single(model => model.Id == current.MachineId).DisplayResourceKey,
            adapter.SupportsPointerCapture(current)
        );
    }

    public string GetFirmwareDirectory(string machineId)
    {
        _ = ModelCatalog.Parse(machineId);
        return _firmwareDirectory;
    }

    public ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var current = Require(configuration);
        if (current.Model != ModelCatalog.Parse(machineId))
            throw new ArgumentException(nameof(machineId));
        var adapter = SelectedOrDefault(current);
        var candidates = new List<EmulationFirmwareCandidate>();
        CommonCoreHostFunctions.FirmwareProcessing.ScanRecursive(GetFirmwareDirectory(machineId), path => candidates.AddRange(adapter.ScanFirmwareFileAsync(current, path, cancellationToken).GetAwaiter().GetResult()), cancellationToken);
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>(candidates);
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware) => FirmwareFunctions.Select(Require(configuration), firmware);
    private IEmulatorAdapter PreferredAdapter(string machineId) => this.Adapters(ModelCatalog.Parse(machineId).ToString()).FirstOrDefault() ?? throw new ArgumentOutOfRangeException(nameof(machineId));
    private IEmulatorAdapter SelectedOrDefault(MachineConfiguration configuration) => configuration.Core == Emulator.None ? PreferredAdapter(configuration.MachineId) : this.Adapter(configuration);
    private static MachineConfiguration Require(IEmulationConfiguration configuration) => configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
    /// <summary>Décrit les capacités de stockage réelles par le même contrat Common que les autres modules.</summary>
    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration)
    {
        var current = Require(configuration);
        return SelectedAdapter(current)?.DescribeStorage(current) ?? StorageSettingsFunctions.Describe(current);
    }

    /// <summary>Applique le contrat de stockage sans ajouter de lecteur à la machine.</summary>
    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings)
    {
        var current = Require(configuration);
        return SelectedAdapter(current)?.ApplyStorage(current, settings) ?? StorageSettingsFunctions.Apply(current, settings);
    }
}
