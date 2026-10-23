using GWGUI.Emulation.Constants;
using GWGUI.Emulation;

using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using AudioFunctions = GWGUI.Emulation.Acorn.Common.Functions.AudioFunctions;
using System.Globalization;
using System.IO;
using GWGUI.Emulation.Acorn.Modules;

namespace GWGUI.Emulation.Acorn.Common.Services;

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
        return new AcornEmulationModule( Path.Combine(context.ModuleDirectory, EmulationModuleConstants.ConfigurationsDirectoryName), context.DataDirectory, context.HttpClient, Path.Combine(context.ModuleDirectory, EmulationModuleConstants.CoreDirectoryName));
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
        if (!ConfigurationFunctions.SupportsRuntime(configuration.Model))
            throw new EmulationException(ErrorCategory.Core, ErrorCode.CoreNotFound, ErrorMessages.CoreFileMissing);
        return _adapters.TryGetValue(EmulatorCatalog.Get(configuration.Core).Id, out var adapter) ? adapter.Create(configuration, context) : throw new ArgumentOutOfRangeException(nameof(configuration));
    }

    internal IEmulatorAdapter Adapter(MachineConfiguration configuration) => _adapters[EmulatorCatalog.Get(configuration.Core).Id];
    internal IEmulatorAdapter Adapter(Emulator emulator) => Adapter(EmulatorCatalog.Get(emulator).Id);

    internal IEmulatorAdapter Adapter(string emulatorId) => _adapters.TryGetValue(emulatorId, out var adapter) ? adapter : throw new ArgumentOutOfRangeException(nameof(emulatorId));

    internal IReadOnlyList<IEmulatorAdapter> Adapters(string machineId) => _adapters.Values.Where(adapter => adapter.Definition.MachineIds.Contains(machineId)).OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).ToArray();

    internal bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        foreach (var adapter in _adapters.Values)
            if (adapter.TryHandleHostCommand(arguments, out exitCode)) return true;
        exitCode = CoreHostConstants.NativeOperationSuccess;
        return false;
    }

    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(AcornEmulationModule).Assembly, EmulationModuleConstants.LocalizationResourceName);
    private readonly string _firmwareDirectory = null !;
    private readonly ConfigurationStore _store = null !;
    private readonly EmulatorManagementContext _managementContext = null !;
    /// <summary>Configure les opérations du module avec ses chemins et son client HTTP emprunté ; la construction sans paramètres conserve le parcours de catalogue.</summary>
    public Engine(string configurationDirectory, string pathBase, HttpClient httpClient, string coreDirectory) : this()
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _firmwareDirectory = Path.Combine(pathBase, EmulationPathConstants.RootDirectoryName, EmulationPathConstants.MachinesDirectoryName, EmulationModuleConstants.ModuleId, EmulationPathConstants.FirmwareDirectoryName);
        _managementContext = new EmulatorManagementContext(httpClient, coreDirectory);
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    public string BrandImageResourceName => $"{EmulationModuleConstants.AssetResourcePrefix}.acorn.png";
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, tab => tab != EmulationMachineTab.Mouse));

    public bool TryGetString(string key, CultureInfo culture, out string value) => Localization.TryGetString(key, culture, out value);

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
        EmulationMachineSettings result = new EmulationMachineSettings(machineId, new EmulationSettingsVisibility(tabs), SettingsDescriptionFunctions.Create(current));
        if (SelectedAdapter(current)?.SupportsVideoSettings == false && !result.Blocks.Any(block => block.Tab == EmulationMachineTab.Video))
        {
            var adjustedTabs = result.Visibility.Tabs.ToDictionary(pair => pair.Key, pair => pair.Value);
            adjustedTabs[EmulationMachineTab.Video] = false;
            result = result with { Visibility = new EmulationSettingsVisibility(adjustedTabs) };
        }
        return result;
    }

    /// <summary>Consulte les recommandations compatibles avant de préparer une nouvelle configuration.</summary>
    /// <param name="configuration">Choix initial du module, déjà construit avec les données du modèle.</param>
    /// <returns>La configuration initiale sans recommandation différente, ou sa sélection adaptée par le cœur recommandé.</returns>
    /// <remarks>Un choix initial déjà recommandé reste prioritaire. Les autres recommandations sont départagées
    /// par identifiant ordinal, parmi les adaptateurs compatibles uniquement. Cette étape est réservée à la
    /// création initiale ; les changements explicites et les documents persistés conservent leur propre choix.</remarks>
    private MachineConfiguration SelectConfiguration(MachineConfiguration configuration)
    {
        var machineId = ((IEmulationConfiguration)configuration).MachineId;
        var current = SelectedAdapter(configuration);
        if (current?.IsDefaultFor(machineId) == true) return configuration;
        var preferred = _adapters.Values.Where(adapter => adapter.Definition.MachineIds.Contains(machineId))
            .OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).FirstOrDefault(adapter => adapter.IsDefaultFor(machineId));
        if (preferred is null || preferred.EmulatorId == current?.EmulatorId) return configuration;
        return preferred.SelectConfiguration(configuration with { Core = Enum.Parse<Emulator>(preferred.EmulatorKey) });
    }


    /// <summary>Prépare les valeurs initiales du cœur sélectionné par le contrat Common.</summary>
    /// <param name="configuration">Nouvelle configuration, entièrement construite et conservée immuable.</param>
    /// <returns>La même instance si les options restent identiques ; sinon une copie contenant les valeurs préparées.</returns>
    /// <remarks>Le dictionnaire transmis au cœur appartient à cette préparation. Les valeurs existantes,
    /// les données de la configuration et la distinction entre une collection absente et vide sont conservées
    /// lorsqu’aucun changement n’est apporté. Sans adaptateur sélectionné, la configuration reste inchangée.</remarks>
    private MachineConfiguration InitializeOptions(MachineConfiguration configuration)
    {
        var adapter = SelectedAdapter(configuration);
        if (adapter is null) return configuration;
        var original = configuration.Options;
        var options = original is null ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(original, StringComparer.Ordinal);
        adapter.InitializeOptions(configuration, options);
        if (original is null ? options.Count == 0 : options.Count == original.Count && options.All(item => original.TryGetValue(item.Key, out var value) && string.Equals(value, item.Value, StringComparison.Ordinal)))
            return configuration;
        return configuration with { Options = options };
    }


    public IEmulationConfiguration CreateConfiguration(string machineId) => InitializeOptions(SelectConfiguration(new MachineConfiguration(ModelCatalog.Parse(machineId))));
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
            Options = current.Core != Emulator.None && this.Adapter(current) is IEmulatorConfigurationOptions adapter
                ? adapter.NormalizeOptions(current with { Options = options }) : options,
            Folders = folders,
            AudioEnabled = values.TryGetValue(SettingsConstants.AudioEnabled, out var enabled) ? enabled == EmulationSettingConstants.Enabled : current.AudioEnabled
        };
        result = SelectedAdapter(result)?.ApplySettings(result, values) ?? result;
        ConfigurationFunctions.Validate(result);
        return result;
        string? Folder(string id, string? original) => values.TryGetValue(id, out var value) ? value : original;
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration) => Require(configuration).Core == Emulator.None ? new Dictionary<string, string>() : this.Adapter(Require(configuration)).GetRuntimeOptions(Require(configuration));
    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) => ConfigurationSummaryFunctions.Create(Require(configuration));
    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => InputSettingsFunctions.Describe(Require(configuration));
    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => InputSettingsFunctions.Apply(Require(configuration), settings);
    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => SaveConfigurationAsync(configuration, cancellationToken);
    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration)
    {
        var current = Require(configuration);
        return SelectedAdapter(current)?.DescribeStorage(current) ?? StorageSettingsFunctions.Describe(current);
    }
    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings)
    {
        var current = Require(configuration);
        return SelectedAdapter(current)?.ApplyStorage(current, settings) ?? StorageSettingsFunctions.Apply(current, settings);
    }

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false)).Cast<IEmulationConfiguration>().ToArray();
    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => new(_store.SaveAsync(Require(configuration), cancellationToken));
    public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default) => PreferredAdapter(machineId).GetInstallationAsync(_managementContext, cancellationToken);
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
        return ValueTask.FromResult<IEmulationConfiguration>(adapter.SelectConfiguration(current with { Core = Enum.Parse<Emulator>(adapter.EmulatorKey) }));
    }
    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default) => PreferredAdapter(machineId).FindReleasesAsync(_managementContext, cancellationToken);
    public ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => PreferredAdapter(machineId).InstallAsync(_managementContext, release, progress, cancellationToken);

    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default)
    {
        var current = Require(configuration);
        var adapter = PreferredAdapter(current.MachineId);
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
                Media = media.Select(item => EmulationMediaConversionFunctions.ToModule(item, current.Media)).ToArray()
            };
            return this.CreateMachine(selected, context);
        }, storage.AvailableDevices, storage.MountedMedia,
            HardwareModelCatalog.Get(current.Model).DisplayNameResourceKey, adapter.SupportsPointerCapture(current));
    }

    private IEmulatorAdapter PreferredAdapter(string machineId) => this.Adapters(ModelCatalog.Parse(machineId).ToString()).SingleOrDefault() ?? throw new ArgumentOutOfRangeException(nameof(machineId));
    private static MachineConfiguration Require(IEmulationConfiguration configuration) => configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
    /// <summary>Résout l’adaptateur sélectionné sans imposer un cœur aux configurations encore incomplètes.</summary>
    /// <param name="configuration">Configuration empruntée portant la sélection enregistrée.</param>
    /// <returns>Adaptateur du catalogue existant, ou null sans sélection correspondante.</returns>
    internal IEmulatorAdapter? SelectedAdapter(MachineConfiguration configuration) => _adapters.Values.FirstOrDefault(candidate => candidate.EmulatorKey == configuration.Core.ToString());

    /// <summary>Retourne le dossier de firmware du module après validation de la machine.</summary>
    public string GetFirmwareDirectory(string machineId)
    {
        _ = ModelCatalog.Parse(machineId);
        return _firmwareDirectory;
    }

    /// <summary>Inspecte les fichiers par le contrat du backend ; un firmware intégré ne fournit aucun candidat externe.</summary>
    public async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current = Require(configuration);
        if (current.MachineId != machineId) throw new ArgumentException(nameof(machineId));
        var directory = GetFirmwareDirectory(machineId);
        if (SelectedAdapter(current) is not { } adapter) return [];
        var candidates = await CommonCoreHostFunctions.FirmwareProcessing.ScanDirectoryAsync(directory,
            (path, token) => adapter.ScanFirmwareFileAsync(current, path, token), cancellationToken).ConfigureAwait(false);
        return candidates.SelectMany(candidateSet => candidateSet).ToArray();
    }

    /// <summary>Revalide le candidat et transmet son rôle au parcours de réglages existant.</summary>
    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware)
    {
        var current = Require(configuration);
        var adapter = SelectedAdapter(current) ?? throw new ArgumentException(nameof(configuration));
        var scanned = CommonCoreHostFunctions.FirmwareProcessing.SelectCandidate(adapter.ScanFirmwareFileAsync(current, firmware.Path, CancellationToken.None).GetAwaiter().GetResult(), firmware.Id, firmware.DestinationFieldId);
        if (scanned.DestinationFieldId is not { } field || scanned.Compatibility == EmulationFirmwareCompatibility.Incompatible) throw new ArgumentException(nameof(firmware));
        return ApplySettings(current, new Dictionary<string, string?> { [field] = scanned.Path });
    }
}
