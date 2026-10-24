using GWGUI.Emulation;

using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Atari.Common.Functions.CoreHostFunctions;
using System.IO;
using GWGUI.Emulation.Atari.Modules;

namespace GWGUI.Emulation.Atari.Common.Services;

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
        return new AtariEmulationModule(Path.Combine(context.ModuleDirectory, "Configurations"), context.DataDirectory, context.HttpClient, Path.Combine(context.ModuleDirectory, "Core"));
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
        return _adapters.TryGetValue(EmulatorCatalog.Get(configuration.Core).Id, out var adapter) ? adapter.Create(configuration, context) : throw new ArgumentOutOfRangeException(nameof(configuration));
    }

    /// <summary>Retrouve l’adaptateur sélectionné sans charger de cœur ; null conserve le parcours sans sélection.</summary>
    internal IEmulatorAdapter? SelectedAdapter(MachineConfiguration configuration) => _adapters.Values.FirstOrDefault(adapter => adapter.EmulatorKey == configuration.Core.ToString());

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

    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(AtariEmulationModule).Assembly, "GWGUI.Emulation.Atari.Resources.Emulation");

    public bool TryGetString(string key, System.Globalization.CultureInfo culture, out string value) => Localization.TryGetString(key, culture, out value);
    private readonly ConfigurationStore _store = null !;
    private readonly HttpClient _httpClient = null !;
    private readonly string _coreDirectory = null !;
    private readonly string _firmwareDirectory = null !;
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
    public string BrandImageResourceName => $"{EmulationModuleConstants.AssetResourcePrefix}.atari.png";
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));

    private EmulatorManagementContext ManagementContext => new(_httpClient, _coreDirectory);

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
        var current = configuration as MachineConfiguration ?? (MachineConfiguration)CreateConfiguration(machineId);
        EmulationMachineSettings result = new EmulationMachineSettings(machineId, new EmulationSettingsVisibility(tabs), this.SelectedAdapter(current)?.GetMachineSettingsBlocks(current) ?? SettingsDescriptionFunctions.Create(current), SettingsRules(model));
        if (SelectedAdapter(current)?.SupportsVideoSettings == false && !result.Blocks.Any(block => block.Tab == EmulationMachineTab.Video))
        {
            var adjustedTabs = result.Visibility.Tabs.ToDictionary(pair => pair.Key, pair => pair.Value);
            adjustedTabs[EmulationMachineTab.Video] = false;
            result = result with { Visibility = new EmulationSettingsVisibility(adjustedTabs) };
        }
        return result;
    }

    private static IReadOnlyList<EmulationSettingsRule> SettingsRules(MachineModel model)
    {
        if (!EightBitSettingsCatalog.SupportsOriginalComputerOptions(model)) return [];
        return
        [
            new(EmulationSettingsRuleCategory.MutuallyExclusive, EightBitSettingsConstants.MosaicMemoryOptionKey, EightBitSettingsConstants.AxlonMemoryOptionKey, EightBitSettingsConstants.Disabled),
            new(EmulationSettingsRuleCategory.VisibleWhenSourceDiffers, EightBitSettingsConstants.AxlonMemoryOptionKey, EightBitSettingsConstants.AxlonShadowOptionKey, EightBitSettingsConstants.Disabled)
        ];
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
        if (configuration is not MachineConfiguration)
            throw new ArgumentException(nameof(configuration));
        return new MachineConfiguration(ModelCatalog.Parse(machineId));
    }

    /// <summary>Transmet le choix graphique au contrat vidéo de l'adaptateur sélectionné.</summary>
    /// <param name="configuration">Configuration empruntée appartenant à cette famille.</param>
    /// <param name="graphicsApi">Choix graphique fourni par le présentateur de l'hôte.</param>
    /// <returns>Configuration adaptée par le cœur, ou inchangée lorsque ce choix ne modifie aucune option native.</returns>
    public IEmulationConfiguration ApplyVideoSettings(IEmulationConfiguration configuration, EmulationGraphicsApi graphicsApi)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return SelectedAdapter(current)?.ApplyVideoSettings(current, graphicsApi) ?? current;
    }

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        if (configuration is not MachineConfiguration atari)
            throw new ArgumentException(nameof(configuration));
        var options = new Dictionary<string, string>(atari.Options);
        CommonCoreHostFunctions.OptionProcessing.ApplyValues(options, values,
            optionKey => optionKey is SettingsConstants.AudioEnabled
                or SettingsConstants.SystemFirmware or SettingsConstants.BasicFirmware
                or SettingsConstants.XegsFirmware or SettingsConstants.HardDiskFolder
                or SettingsConstants.CpuOriginalFrequency);
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
        var result = atari with
        {
            Firmwares = firmwares, Options = options, Folders = folders,
            AudioEnabled = values.GetValueOrDefault(SettingsConstants.AudioEnabled)
                == SettingsDescriptionFunctionsConstants.Enabled
        };
        result = SelectedAdapter(result)?.ApplySettings(result, values) ?? result;
        return result;
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration)
    {
        var atari = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return this.Adapter(atari).GetRuntimeOptions(atari);
    }



    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return SelectedAdapter(current)?.Summarize(current) ?? ConfigurationSummaryFunctions.Create(current);
    }

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => InputSettingsFunctions.Describe(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => InputSettingsFunctions.Apply(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is MachineConfiguration atari ? new ValueTask(_store.SaveAsync(atari, cancellationToken)) : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return SelectedAdapter(current)?.DescribeStorage(current) ?? StorageSettingsFunctions.Describe(current);
    }

    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return SelectedAdapter(current)?.ApplyStorage(current, settings) ?? StorageSettingsFunctions.Apply(current, settings);
    }

    private static IReadOnlyList<FirmwareConfiguration> ApplyFirmware(MachineConfiguration configuration, IReadOnlyList<FirmwareConfiguration> firmwares, string fieldId, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return FirmwareSelectionFunctions.ReplaceField(configuration.Model, firmwares, fieldId, null);
        var scanned = FirmwareScanFunctions.ScanFileAsync(path, configuration.Model, null, CancellationToken.None).GetAwaiter().GetResult();
        var selected = FirmwareScanFunctions.CreateSelection(scanned);
        if (!string.Equals(FirmwareSelectionFunctions.FieldId(configuration.Model, selected.Category), fieldId, StringComparison.Ordinal))
            throw new InvalidOperationException(nameof(path));
        return FirmwareSelectionFunctions.ReplaceField(configuration.Model, firmwares, fieldId, selected);
    }

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false)).Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is MachineConfiguration atari ? new ValueTask(_store.SaveAsync(atari, cancellationToken)) : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default) => this.Adapter(ConfigurationFunctions.GetCore(ModelCatalog.Parse(machineId))).GetInstallationAsync(ManagementContext, cancellationToken);

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) =>
        this.Adapter(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration))).GetInstallationAsync(ManagementContext, cancellationToken);

    public async ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var atari = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var results = new List<EmulationEmulatorInstallation>();
        foreach (var adapter in this.Adapters(atari.MachineId))
            results.Add(await adapter.GetInstallationAsync(ManagementContext, cancellationToken).ConfigureAwait(false));
        return results;
    }

    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var atari = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var adapter = this.Adapter(emulatorId);
        if (!adapter.Definition.MachineIds.Contains(atari.MachineId))
            throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);
        return ValueTask.FromResult<IEmulationConfiguration>(adapter.SelectConfiguration(atari with { Core = Enum.Parse<Emulator>(adapter.EmulatorKey) }));
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default) => this.Adapter(ConfigurationFunctions.GetCore(ModelCatalog.Parse(machineId))).FindReleasesAsync(ManagementContext, cancellationToken);

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) =>
        this.Adapter(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration))).FindReleasesAsync(ManagementContext, cancellationToken);

    public ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) => this.Adapter(ConfigurationFunctions.GetCore(ModelCatalog.Parse(machineId)))
        .InstallAsync(ManagementContext, release, progress, cancellationToken);

    public ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default) =>
        this.Adapter(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration))).InstallAsync(ManagementContext, release, progress, cancellationToken);

    public string GetFirmwareDirectory(string machineId)
    {
        var family = StorageConfigurationFunctions.Family(ModelCatalog.Parse(machineId));
        return Path.Combine(_firmwareDirectory, FirmwareScanFunctions.FamilyDirectoryName(family));
    }

    public async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var atari = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        if (SelectedAdapter(atari) is { } adapter) return await adapter.ScanFirmwareAsync(atari, GetFirmwareDirectory(machineId), cancellationToken).ConfigureAwait(false);
        var paths = FirmwareScanFunctions.EnumerateCandidates(_firmwareDirectory).Where(path => string.Equals(Path.GetDirectoryName(path), GetFirmwareDirectory(machineId), StringComparison.OrdinalIgnoreCase));
        var entries = new List<EmulationFirmwareCandidate>();
        foreach (var path in paths)
            entries.AddRange(await ScanFirmwareFileAsync(atari, path, cancellationToken).ConfigureAwait(false));
        return entries;
    }

    /// <summary>Inspecte un fichier par les catalogues existants et conserve sa projection vers le champ de firmware.</summary>
    /// <param name="configuration">Configuration empruntée portant le modèle et les options.</param>
    /// <param name="path">Fichier source ouvert uniquement par le lecteur existant.</param>
    /// <param name="cancellationToken">Annulation transmise aux lectures et empreintes.</param>
    /// <returns>Le candidat avec la même identité, le même libellé et la même destination que dans le parcours de dossier.</returns>
    internal static async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken)
    {
        var scanned = await FirmwareScanFunctions.ScanFileAsync(path, configuration.Model, null, cancellationToken).ConfigureAwait(false);
        var name = scanned.Definition?.Category == FirmwareCategory.Tos ? $"{MachineConfigurationConstants.TosLabel} {scanned.Definition.Version}" : Path.GetFileName(scanned.Path);
        var compatibility = scanned.Compatibility;
        return [new EmulationFirmwareCandidate(scanned.Md5 ?? scanned.Path, scanned.Path, name, null,
            compatibility, compatibility == EmulationFirmwareCompatibility.Incompatible || scanned.Definition?.Category is null ? null : FirmwareSelectionFunctions.FieldId(configuration.Model, scanned.Definition.Category.Value) ?? SettingsConstants.SystemFirmware)];
    }

    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware)
    {
        var atari = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        if (firmware.DestinationFieldId is not (SettingsConstants.SystemFirmware
            or SettingsConstants.BasicFirmware or SettingsConstants.XegsFirmware))
            throw new InvalidOperationException(nameof(firmware));
        var scanned = FirmwareScanFunctions.ScanFileAsync(firmware.Path, atari.Model, null, CancellationToken.None).GetAwaiter().GetResult();
        var selected = FirmwareScanFunctions.CreateSelection(scanned);
        var actualFieldId = FirmwareSelectionFunctions.FieldId(atari.Model, selected.Category);
        var configured = actualFieldId is null ? atari.Firmwares.Where(item => item.Category != selected.Category).Append(selected).ToArray() : FirmwareSelectionFunctions.ReplaceField(atari.Model, atari.Firmwares, actualFieldId, selected);
        return atari with { Firmwares = configured };
    }


    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration atari)
            throw new ArgumentException(nameof(configuration));
        var adapter = this.Adapter(atari);
        var corePath = await adapter.FindInstalledCorePathAsync(ManagementContext, cancellationToken).ConfigureAwait(false)
            ?? throw CommonCoreHostFunctions.Messages.EmulatorNotInstalled(adapter.EmulatorId);
        cancellationToken.ThrowIfCancellationRequested();
        atari = await adapter.PrepareRuntimeConfigurationAsync(atari, services.ConvertedMediaDirectory).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var audio = adapter.GetAudioConfiguration(atari);
        var creationContext = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
            value => Path.Combine(services.StatesDirectory, value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)));
        var storage = StorageSettingsFunctions.Describe(atari);
        var devices = storage.AvailableDevices.Where(device => storage.ConfiguredSlots.Contains(device.Slot)).ToArray();
        var mounted = atari.Media.Select(EmulationMediaConversionFunctions.ToCommon).OfType<EmulationMedia>().ToArray();
        return new EmulationMachineRuntime(atari,
            CreateMachineFactory(this, atari, creationContext), devices, mounted,
            MachineCatalog.All.First(model => model.Id == atari.MachineId).DisplayResourceKey,
            adapter.SupportsPointerCapture(atari),
            PrepareMediaAsync: (media, token) => { token.ThrowIfCancellationRequested(); return adapter.PrepareMediaAsync(media, services.ConvertedMediaDirectory); });
    }

    private static Func<IReadOnlyList<EmulationMedia>, IEmulatedMachine> CreateMachineFactory(Engine engine, MachineConfiguration configuration, EmulatorCreationContext context) => media => engine.CreateMachine(WithMedia(configuration, media), context);

    private static MachineConfiguration WithMedia(MachineConfiguration configuration, IEnumerable<EmulationMedia> media)
    {
        var converted = media.Select(item => EmulationMediaConversionFunctions.ToModule(item, configuration.Media)).ToArray();
        return configuration with { Media = converted };
    }
}
