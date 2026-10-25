using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Thomson.Common.Dictionaries;

using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Thomson.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation;
using System.Globalization;
using System.IO;
using System.Net.Http;
using GWGUI.Emulation.Thomson.Modules;

namespace GWGUI.Emulation.Thomson.Common.Services;

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
        return new ThomsonEmulationModule(Path.Combine(context.ModuleDirectory, EmulationModuleConstants.ConfigurationsDirectoryName), context.DataDirectory, context.HttpClient, Path.Combine(context.ModuleDirectory, EmulationModuleConstants.CoreDirectoryName));
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

    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(ThomsonEmulationModule).Assembly, EmulationModuleConstants.LocalizationResourceName);
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
    public string BrandImageResourceName => $"{EmulationModuleConstants.AssetResourcePrefix}.thomson.png";
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, tab => tab != EmulationMachineTab.Rom));

    public bool TryGetString(string key, CultureInfo culture, out string value) => Localization.TryGetString(key, culture, out value);
    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var model = ModelCatalog.Parse(machineId);
        var current = configuration is null ? new MachineConfiguration(model) : Require(configuration);
        if (current.Model != model)
            throw new ArgumentException(nameof(configuration));
        EmulationMachineSettings result = new EmulationMachineSettings(machineId, DefaultVisibility, SettingsDescriptionFunctions.Create(current));
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
        var current = Require(configuration);
        var editable = CommonCoreHostFunctions.OptionProcessing.EditableFieldIds(SettingsDescriptionFunctions.Create(current));
        var options = new Dictionary<string, string>(current.Options, StringComparer.Ordinal);
        CommonCoreHostFunctions.OptionProcessing.ApplyValues(options, values,
            optionKey => !editable.Contains(optionKey) || optionKey is SettingsConstants.AudioEnabled or SettingsConstants.MouseSpeed);

        var result = current with
        {
            Options = options,
            Input = values.TryGetValue(SettingsConstants.MouseSpeed, out var speed) && editable.Contains(SettingsConstants.MouseSpeed)
                ? current.Input with
            {
                MouseSpeedPercent = int.Parse(speed ?? InputMappingConstants.DefaultMouseSpeedPercent.ToString(CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture)
                }


                : current.Input,
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

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) {
        var current = Require(configuration);
        return SelectedAdapter(current)?.Summarize(current) ?? ConfigurationSummaryFunctions.Create(current);
    }
    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => InputSettingsFunctions.Describe(Require(configuration));
    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => InputSettingsFunctions.Apply(Require(configuration), settings);
    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => SaveConfigurationAsync(configuration, cancellationToken);
    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration)
    {
        var current = Require(configuration);
        var adapter = SelectedAdapter(current);
        var settings = adapter?.DescribeStorage(current) ?? StorageSettingsFunctions.Describe(current);
        return CommonCoreHostFunctions.MediaRules.RestrictAcceptedExtensions(settings, EmulationMediaCategory.CartridgeSlot, adapter?.GetCartridgeExtensions(current));
    }
    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration, EmulationStorageSettings settings)
    {
        var current = Require(configuration);
        var adapter = SelectedAdapter(current);
        if (adapter?.GetCartridgeExtensions(current) is not null)
        {
            var devices = DescribeStorageSettings(current).AvailableDevices;
            if (settings.MountedMedia.Any(media => media.Slot.Category == EmulationMediaCategory.CartridgeSlot && CommonCoreHostFunctions.MediaRules.IsInvalidMountedMedia(media, devices, settings.ConfiguredSlots)))
                throw new ArgumentException(nameof(settings));
        }
        return adapter?.ApplyStorage(current, settings) ?? StorageSettingsFunctions.Apply(current, settings);
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
        cancellationToken.ThrowIfCancellationRequested();
        current = await adapter.PrepareRuntimeConfigurationAsync(current, services.ConvertedMediaDirectory).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var audio = adapter.GetAudioConfiguration(current);
        var context = new EmulatorCreationContext(
            services.SessionsDirectory,
            corePath,
            services.HostExecutablePath,
            () => services.CreateAudioOutput(audio.OutputDeviceId, audio.LatencyMilliseconds),
            value => Path.Combine(services.StatesDirectory, EmulationModuleConstants.SavesDirectoryName, value.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat))
        );
        var storage = DescribeStorageSettings(current);
        return new EmulationMachineRuntime(
            current,
            media => this.CreateMachine(current with { Media = media.Select(item => EmulationMediaConversionFunctions.ToModule(item, current.Media)).ToArray() }, context),
            storage.AvailableDevices,
            storage.MountedMedia,
            ModelCatalog.All.Single(model => model.Id == current.MachineId).DisplayResourceKey,
            adapter.SupportsPointerCapture(current)
        ,
            PrepareMediaAsync: (media, token) => { token.ThrowIfCancellationRequested(); return adapter.PrepareMediaAsync(media, services.ConvertedMediaDirectory); });
    }

    private IEmulatorAdapter PreferredAdapter(string machineId) => this.Adapters(ModelCatalog.Parse(machineId).ToString()).FirstOrDefault() ?? throw new ArgumentOutOfRangeException(nameof(machineId));
    private IEmulatorAdapter SelectedOrDefault(MachineConfiguration configuration) => configuration.Core == Emulator.None ? PreferredAdapter(configuration.MachineId) : this.Adapter(configuration);
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
        return await adapter.ScanFirmwareAsync(current, directory, cancellationToken).ConfigureAwait(false);
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
