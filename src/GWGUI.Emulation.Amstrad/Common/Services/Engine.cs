using GWGUI.Emulation.Constants;
using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Amstrad.Common.Functions.CoreHostFunctions;
using System.IO;
using GWGUI.Emulation.Amstrad.Modules;

namespace GWGUI.Emulation.Amstrad.Common.Services;

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
        Directory.CreateDirectory(Path.Combine(context.ModuleDirectory, EmulationPathConstants.FirmwareDirectoryName));
        return new AmstradEmulationModule(
            Path.Combine(context.ModuleDirectory, ConfigurationStoreConstants.ConfigurationsDirectoryName),
            context.DataDirectory, context.HttpClient,
            Path.Combine(context.ModuleDirectory, EmulationModuleConstants.CoreDirectoryName));
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

    internal IEmulatorAdapter Adapter(MachineConfiguration configuration) => Adapter(configuration.EmulatorId);


    internal IEmulatorAdapter Adapter(string emulatorId) => _adapters.TryGetValue(emulatorId, out var adapter) ? adapter : throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);

    internal bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        foreach (var adapter in _adapters.Values)
            if (adapter.TryHandleHostCommand(arguments, out exitCode)) return true;
        exitCode = EmulationModuleConstants.DefaultHostExitCode;
        return false;
    }

    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(AmstradEmulationModule).Assembly, CoreErrorResourceKeys.ResourceName);
    private readonly string _firmwareDirectory = null !;
    private readonly ConfigurationStore _store = null !;
    private readonly HttpClient _httpClient = null !;
    private readonly string _coreDirectory = null !;
    private EmulatorManagementContext EmulatorManagement(IEmulatorAdapter adapter) => new(_httpClient, Path.Combine(_coreDirectory, adapter.EmulatorId));
    /// <summary>Configure les opérations du module avec ses chemins et son client HTTP emprunté ; la construction sans paramètres conserve le parcours de catalogue.</summary>
    public Engine(string configurationDirectory, string pathBase, HttpClient httpClient, string coreDirectory) : this()
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _firmwareDirectory = Path.Combine(pathBase, EmulationPathConstants.RootDirectoryName, EmulationPathConstants.MachinesDirectoryName, EmulationModuleConstants.ModuleId, EmulationPathConstants.FirmwareDirectoryName);
        _httpClient = httpClient;
        _coreDirectory = coreDirectory;
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    public string BrandImagePath => $"{EmulationModuleConstants.AssetPathPrefix}{EmulationModuleConstants.ModuleIconPathSuffix}";
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));

    public bool TryGetString(string key, System.Globalization.CultureInfo culture, out string value) => Localization.TryGetString(key, culture, out value);

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var current = configuration as MachineConfiguration ?? (MachineConfiguration)CreateConfiguration(machineId);
        var model = ModelCatalog.Get(current.Model);
        var blocks = SettingsDescriptionFunctions.Create(current, EmulatorManagement(this.Adapter(current)));
        var capabilities = this.Adapter(current);
        var tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
        {
            EmulationMachineTab.Keyboard => model.HasKeyboard,
            EmulationMachineTab.Mouse => capabilities.SupportsPointer(current) && model.MouseButtonCount > BufferConstants.EmptyCollectionCount,
            EmulationMachineTab.Storage => model.MaximumFloppyDriveCount > BufferConstants.EmptyCollectionCount
                || model.SupportsCassetteDrive || model.SupportsCartridgeSlot,
            _ => item.Value
        });
        EmulationMachineSettings result = new EmulationMachineSettings(model.Id, new EmulationSettingsVisibility(tabs), blocks);
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
        return preferred.SelectConfiguration(configuration with { EmulatorId = preferred.EmulatorId });
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


    public IEmulationConfiguration CreateConfiguration(string machineId)
    {
        var model = ModelCatalog.Get(machineId);
        return InitializeOptions(SelectConfiguration(new MachineConfiguration(model.Id, DefaultEmulatorId(model.Id),
            Options: new Dictionary<string, string>(StringComparer.Ordinal), Id: Guid.NewGuid(),
            Controllers: Enumerable.Repeat(ControllerCatalog.Default(model), model.ControllerPortCount).ToArray(),
            Input: new InputConfiguration(), Media: [])));
    }

    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId)
    {
        if (configuration is not MachineConfiguration current)
            throw new ArgumentException(nameof(configuration));
        var created = (MachineConfiguration)CreateConfiguration(machineId);
        var emulatorId = EmulatorCatalog.GetAll(machineId).Any(item => item.Id == current.EmulatorId) ? current.EmulatorId : created.EmulatorId;
        return created with { Id = current.Id, EmulatorId = emulatorId,
            AudioEnabled = current.AudioEnabled, Audio = current.Audio };
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
        var amstrad = RequireConfiguration(configuration);
        var options = new Dictionary<string, string>(amstrad.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        CommonCoreHostFunctions.OptionProcessing.ApplyValues(options, values,
            optionKey => optionKey is SettingsConstants.Model or SettingsConstants.Emulator
                or SettingsConstants.AudioEnabled or SettingsConstants.AudioOutput
                or SettingsConstants.AudioLatency || optionKey.StartsWith(SettingsConstants.Model + ConfigurationStoreConstants.TemporaryNameSeparator, StringComparison.Ordinal));
        var audio = SelectedAdapter(amstrad)?.GetAudioConfiguration(amstrad) ?? amstrad.Audio ?? new AudioConfiguration();
        var result = amstrad with
        {
            Options = options,
            AudioEnabled = values.TryGetValue(SettingsConstants.AudioEnabled, out var enabled) ? enabled == SettingsDescriptionFunctionsConstants.Enabled : amstrad.AudioEnabled,
            Audio = audio with
            {
                OutputDeviceId = values.TryGetValue(SettingsConstants.AudioOutput, out var output) ? string.IsNullOrWhiteSpace(output) ? null : output : audio.OutputDeviceId,
                LatencyMilliseconds = values.TryGetValue(SettingsConstants.AudioLatency, out var latency) && int.TryParse(latency, out var parsedLatency) ? parsedLatency : audio.LatencyMilliseconds
            }
        };
        result = SelectedAdapter(result)?.ApplySettings(result, values) ?? result;
        return result;
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var adapter = string.IsNullOrEmpty(current.EmulatorId) ? null : _adapters.GetValueOrDefault(current.EmulatorId);
        return adapter?.GetRuntimeOptions(current) ?? current.Options ?? new Dictionary<string, string>();
    }

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return SelectedAdapter(current)?.Summarize(current) ?? ConfigurationSummaryFunctions.Create(current);
    }

    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var adapter = SelectedAdapter(current);
        var settings = adapter?.DescribeStorage(current) ?? StorageSettingsFunctions.Describe(current);
        return CommonCoreHostFunctions.MediaRules.RestrictAcceptedExtensions(settings, EmulationMediaCategory.CartridgeSlot, adapter?.GetCartridgeExtensions(current));
    }

    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var adapter = SelectedAdapter(current);
        if (adapter?.GetCartridgeExtensions(current) is not null)
        {
            var devices = DescribeStorageSettings(current).AvailableDevices;
            if (settings.MountedMedia.Any(media => media.Slot.Category == EmulationMediaCategory.CartridgeSlot && CommonCoreHostFunctions.MediaRules.IsInvalidMountedMedia(media, devices, settings.ConfiguredSlots)))
                throw new ArgumentException(nameof(settings));
        }
        return adapter?.ApplyStorage(current, settings) ?? StorageSettingsFunctions.Apply(current, settings);
    }

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => DescribeInput(RequireConfiguration(configuration));

    private EmulationInputSettings DescribeInput(MachineConfiguration configuration)
    {
        var settings = InputSettingsFunctions.Describe(configuration);
        return this.Adapter(configuration) is IEmulatorPointerCapabilities capabilities && !capabilities.SupportsPointer(configuration) ? settings with { Mouse = null } : settings;
    }

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => InputSettingsFunctions.Apply(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is MachineConfiguration amstrad ? new ValueTask(_store.SaveAsync(amstrad, cancellationToken)) : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false)).Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration amstrad)
            return ValueTask.FromException(new ArgumentException(nameof(configuration)));
        ConfigurationValidationFunctions.ValidateForSave(amstrad);
        return new ValueTask(_store.SaveAsync(amstrad, cancellationToken));
    }

    public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var adapter = this.Adapter(DefaultEmulatorId(machineId));
        return adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken);
    }

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var adapter = this.Adapter(RequireConfiguration(configuration).EmulatorId);
        return adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken);
    }

    public async ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var amstrad = RequireConfiguration(configuration);
        var installations = new List<EmulationEmulatorInstallation>();
        foreach (var emulator in EmulatorCatalog.GetAll(amstrad.Model))
        {
            var adapter = this.Adapter(emulator.Id);
            installations.Add(await adapter.GetInstallationAsync(EmulatorManagement(adapter), cancellationToken).ConfigureAwait(false));
        }
        return installations;
    }

    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var amstrad = RequireConfiguration(configuration);
        if (!EmulatorCatalog.GetAll(amstrad.Model).Any(item => item.Id == emulatorId))
            return ValueTask.FromException<IEmulationConfiguration>(new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null));
        var selected = amstrad with { EmulatorId = emulatorId };
        selected = this.Adapter(selected).SelectConfiguration(selected);
        return ValueTask.FromResult<IEmulationConfiguration>(selected);
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var adapter = this.Adapter(DefaultEmulatorId(machineId));
        return adapter.FindReleasesAsync(EmulatorManagement(adapter), cancellationToken);
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var adapter = this.Adapter(RequireConfiguration(configuration).EmulatorId);
        return adapter.FindReleasesAsync(EmulatorManagement(adapter), cancellationToken);
    }

    public ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        _ = ModelCatalog.Get(machineId);
        var adapter = this.Adapter(DefaultEmulatorId(machineId));
        return adapter.InstallAsync(EmulatorManagement(adapter), release, progress, cancellationToken);
    }

    public ValueTask<string> InstallEmulatorAsync(IEmulationConfiguration configuration, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var adapter = this.Adapter(RequireConfiguration(configuration).EmulatorId);
        return adapter.InstallAsync(EmulatorManagement(adapter), release, progress, cancellationToken);
    }

    public async ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default)
    {
        if (configuration is not MachineConfiguration amstrad)
            throw new ArgumentException(nameof(configuration));
        var adapter = this.Adapter(amstrad);
        var corePath = await adapter.FindInstalledCorePathAsync(EmulatorManagement(adapter), cancellationToken).ConfigureAwait(false) ?? throw CommonCoreHostFunctions.Messages.EmulatorNotInstalled(adapter.EmulatorId);
        cancellationToken.ThrowIfCancellationRequested();
        amstrad = await adapter.PrepareRuntimeConfigurationAsync(amstrad, services.ConvertedMediaDirectory).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var audio = adapter.GetAudioConfiguration(amstrad);
        var context = new EmulatorCreationContext(services.SessionsDirectory, corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
            value => Path.Combine(services.StatesDirectory, value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat), CoreDirectoryConstants.SavesDirectoryName));
        var storage = DescribeStorageSettings(amstrad);
        var mounted = adapter.ResolveConfiguredMedia(amstrad);
        return new EmulationMachineRuntime(amstrad,
            media => this.CreateMachine(WithMedia(amstrad, media), context),
            storage.AvailableDevices.Where(device => storage.ConfiguredSlots.Contains(device.Slot)).ToArray(), mounted,
            MachineConfigurationConstants.ResourcePrefix + amstrad.Model,
            SupportsPointerCapture: adapter.SupportsPointerCapture(amstrad),
            PrepareMediaAsync: (media, token) => { token.ThrowIfCancellationRequested(); return adapter.PrepareMediaAsync(media, services.ConvertedMediaDirectory); });
    }

    private static MachineConfiguration WithMedia(MachineConfiguration configuration, IEnumerable<EmulationMedia> media) => configuration with
    {
        Media = media.Select((item, index) => new MediaConfiguration(item.Path,
            item.Type switch
            {
                EmulationMediaType.Floppy => MediaCategory.Floppy,
                EmulationMediaType.Cassette => MediaCategory.Cassette,
                EmulationMediaType.Cartridge => MediaCategory.Cartridge,
                _ => throw new ArgumentOutOfRangeException(nameof(media), item.Type, null)
            }, IsReadOnly: item.IsReadOnly, IsInserted: item.IsInserted,
            MountOrder: index)).ToArray()
    };

    private static string DefaultEmulatorId(string machineId) => EmulatorCatalog.GetAll(machineId).FirstOrDefault()?.Id ?? string.Empty;

    private static MachineConfiguration RequireConfiguration(IEmulationConfiguration configuration) => configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
    /// <summary>Résout l’adaptateur sélectionné sans imposer un cœur aux configurations encore incomplètes.</summary>
    /// <param name="configuration">Configuration empruntée portant la sélection enregistrée.</param>
    /// <returns>Adaptateur du catalogue existant, ou null sans sélection correspondante.</returns>
    internal IEmulatorAdapter? SelectedAdapter(MachineConfiguration configuration) => _adapters.Values.FirstOrDefault(candidate => candidate.EmulatorId == configuration.EmulatorId);

    /// <summary>Retourne le dossier de firmware du module après validation de la machine.</summary>
    public string GetFirmwareDirectory(string machineId)
    {
        _ = ModelCatalog.Get(machineId);
        return _firmwareDirectory;
    }

    /// <summary>Inspecte les fichiers par le contrat du backend ; un firmware intégré ne fournit aucun candidat externe.</summary>
    public async ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareAsync(string machineId, IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current = RequireConfiguration(configuration);
        if (configuration.MachineId != machineId) throw new ArgumentException(nameof(machineId));
        var directory = GetFirmwareDirectory(machineId);
        if (SelectedAdapter(current) is not { } adapter) return [];
        return await adapter.ScanFirmwareAsync(current, directory, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Revalide le candidat et transmet son rôle au parcours de réglages existant.</summary>
    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware)
    {
        var current = RequireConfiguration(configuration);
        var adapter = SelectedAdapter(current) ?? throw new ArgumentException(nameof(configuration));
        var scanned = CommonCoreHostFunctions.FirmwareProcessing.SelectCandidate(adapter.ScanFirmwareFileAsync(current, firmware.Path, CancellationToken.None).GetAwaiter().GetResult(), firmware.Id, firmware.DestinationFieldId);
        if (scanned.DestinationFieldId is not { } field || scanned.Compatibility == EmulationFirmwareCompatibility.Incompatible) throw new ArgumentException(nameof(firmware));
        return ApplySettings(current, new Dictionary<string, string?> { [field] = scanned.Path });
    }
}
