using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using AudioFunctions = GWGUI.Emulation.Acorn.Common.Functions.AudioFunctions;
using GWGUI.Emulation;
using System.Globalization;
using System.IO;

namespace GWGUI.Emulation.Acorn.Modules;

public sealed class AcornEmulationModule : IEmulationModule, IEmulationEmulatorManager,
    IEmulationInputSettingsManager, IEmulationStorageSettingsManager, IEmulationModuleLocalization
{
    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(AcornEmulationModule).Assembly, EmulationModuleConstants.LocalizationResourceName);
    private readonly ConfigurationStore _store;
    private readonly EmulatorManagementContext _managementContext;
    private readonly Engine _engine = new();

    public AcornEmulationModule(string configurationDirectory, string pathBase, HttpClient httpClient, string coreDirectory)
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _managementContext = new EmulatorManagementContext(httpClient, coreDirectory);
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    public string BrandImageResourceName => $"{EmulationModuleConstants.AssetResourcePrefix}.acorn.png";
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, tab => tab != EmulationMachineTab.Mouse));

    public bool TryGetString(string key, CultureInfo culture, out string value) => Localization.TryGetString(key, culture, out value);
    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode) => _engine.TryHandleHostCommand(arguments, out exitCode);

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var model = ModelCatalog.Parse(machineId);
        var current = configuration is null ? new MachineConfiguration(model) : Require(configuration);
        if (current.Model != model) throw new ArgumentException(nameof(configuration));
        var tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
        {
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
        var editable = CommonCoreHostFunctions.OptionProcessing.EditableFieldIds(SettingsDescriptionFunctions.Create(current));
        var options = new Dictionary<string, string>(current.Options, StringComparer.Ordinal);
        CommonCoreHostFunctions.OptionProcessing.ApplyValues(options, values,
            optionKey => !editable.Contains(optionKey) || optionKey is SettingsConstants.AudioEnabled
                or SettingsConstants.SharedFolder or SettingsConstants.FloppyFolder
                or SettingsConstants.StateFolder or SettingsConstants.CaptureFolder);
        var folders = current.Folders with
        {
            Shared = Folder(SettingsConstants.SharedFolder, current.Folders.Shared),
            Floppies = Folder(SettingsConstants.FloppyFolder, current.Folders.Floppies),
            States = Folder(SettingsConstants.StateFolder, current.Folders.States),
            Captures = Folder(SettingsConstants.CaptureFolder, current.Folders.Captures)
        };
        var result = current with
        {
            Options = current.Core != Emulator.None && _engine.Adapter(current) is IEmulatorConfigurationOptions adapter
                ? adapter.NormalizeOptions(current.Model, options) : options,
            Folders = folders,
            AudioEnabled = values.TryGetValue(SettingsConstants.AudioEnabled, out var enabled) ? enabled == EmulationSettingConstants.Enabled : current.AudioEnabled
        };
        ConfigurationFunctions.Validate(result);
        return result;
        string? Folder(string id, string? original) => values.TryGetValue(id, out var value) ? value : original;
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration) => Require(configuration).Core == Emulator.None ? new Dictionary<string, string>() : _engine.Adapter(Require(configuration)).GetRuntimeOptions(Require(configuration));
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
        var adapter = Adapter(current.MachineId);
        var corePath = await adapter.FindInstalledCorePathAsync(_managementContext, cancellationToken).ConfigureAwait(false) ?? throw CommonCoreHostFunctions.Messages.EmulatorNotInstalled(adapter.EmulatorId);
        var output = AudioFunctions.Processing.ResolveRuntimeOutput(current.Options, SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput);
        var latency = AudioFunctions.Processing.ResolveRuntimeLatency(current.Options, SettingsConstants.AudioLatency);
        var context = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath, () => services.CreateAudioOutput(output, latency),
            value => Path.Combine(services.StatesDirectory, value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)));
        var storage = StorageSettingsFunctions.Describe(current);
        return new EmulationMachineRuntime(current, media =>
        {
            var selected = current with
            {
                Media = media.Select(item => EmulationMediaConversionFunctions.ToAcorn(item, current.Media)).ToArray()
            };
            return _engine.CreateMachine(selected, context);
        }, storage.AvailableDevices, storage.MountedMedia,
            HardwareModelCatalog.Get(current.Model).DisplayNameResourceKey, false);
    }

    private IEmulatorAdapter Adapter(string machineId) => _engine.Adapters(ModelCatalog.Parse(machineId).ToString()).SingleOrDefault() ?? throw new ArgumentOutOfRangeException(nameof(machineId));
    private static MachineConfiguration Require(IEmulationConfiguration configuration) => configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
}
