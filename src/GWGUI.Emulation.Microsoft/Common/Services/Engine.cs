using GWGUI.Emulation.Constants;
using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Microsoft.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Microsoft.Common.Constants;
using GWGUI.Emulation.Microsoft.Common.Dictionaries;
using GWGUI.Emulation.Microsoft.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Microsoft.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Microsoft.Common.Services;
using GWGUI.Emulation.Microsoft.Modules;

namespace GWGUI.Emulation.Microsoft.Common.Services;

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
        Directory.CreateDirectory(Path.Combine(context.ModuleDirectory, "Configurations"));
        return new MicrosoftEmulationModule(Path.Combine(context.ModuleDirectory, "Configurations"), context.DataDirectory, context.HttpClient, Path.Combine(context.ModuleDirectory, "Core"));
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

    /// <summary>Retrouve l’adaptateur sélectionné sans charger de cœur ; null conserve le parcours sans sélection.</summary>
    internal IEmulatorAdapter? SelectedAdapter(MachineConfiguration configuration) => string.IsNullOrEmpty(configuration.EmulatorId) ? null : _adapters.GetValueOrDefault(configuration.EmulatorId);

    internal IEmulatorAdapter Adapter(MachineConfiguration configuration) => Adapter(configuration.EmulatorId);

    internal IEmulatorAdapter Adapter(string emulatorId) => _adapters.TryGetValue(emulatorId, out var adapter) ? adapter : throw new ArgumentOutOfRangeException(nameof(emulatorId), emulatorId, null);

    internal bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        foreach (var adapter in _adapters.Values)
            if (adapter.TryHandleHostCommand(arguments, out exitCode)) return true;
        exitCode = 0;
        return false;
    }

    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(MicrosoftEmulationModule).Assembly, "GWGUI.Emulation.Microsoft.Resources.Emulation");
    private readonly string _firmwareDirectory = null !;
    private readonly ConfigurationStore _store = null !;

    public bool TryGetString(string key, System.Globalization.CultureInfo culture, out string value) => Localization.TryGetString(key, culture, out value);
    /// <summary>Configure les opérations du module avec ses chemins et son client HTTP emprunté ; la construction sans paramètres conserve le parcours de catalogue.</summary>
    public Engine(string configurationDirectory, string pathBase, HttpClient httpClient, string coreDirectory) : this()
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
        _firmwareDirectory = Path.Combine(pathBase, EmulationPathConstants.RootDirectoryName, EmulationPathConstants.MachinesDirectoryName, EmulationModuleConstants.ModuleId, EmulationPathConstants.FirmwareDirectoryName);
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    public string BrandImageResourceName => $"{EmulationModuleConstants.AssetResourcePrefix}.microsoft.png";
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var model = ModelCatalog.Get(machineId);
        var current = configuration as MachineConfiguration ?? (MachineConfiguration)CreateConfiguration(machineId);
        var tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
        {
            EmulationMachineTab.Rom => false,
            EmulationMachineTab.Keyboard => model.HasKeyboard,
            EmulationMachineTab.Mouse => model.MouseButtonCount > 0,
            EmulationMachineTab.Storage => model.MaximumFloppyDriveCount > 0
                || model.SupportsCassetteDrive || model.SupportsCartridgeSlot
                || model.SupportsCompactDiscDrive,
            _ => item.Value
        });
        EmulationMachineSettings result = new EmulationMachineSettings(machineId, new EmulationSettingsVisibility(tabs), this.SelectedAdapter(current)?.DescribeSettings(current) ?? SettingsDescriptionFunctions.Create(current));
        if (SelectedAdapter(current)?.SupportsVideoSettings == false && !result.Blocks.Any(block => block.Tab == EmulationMachineTab.Video))
        {
            var adjustedTabs = result.Visibility.Tabs.ToDictionary(pair => pair.Key, pair => pair.Value);
            adjustedTabs[EmulationMachineTab.Video] = false;
            result = result with { Visibility = new EmulationSettingsVisibility(adjustedTabs) };
        }
        return result;
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
        return InitializeOptions(new MachineConfiguration(model.Id, EmulatorCatalog.GetAll(model.Id).FirstOrDefault()?.Id ?? string.Empty,
            Options: new Dictionary<string, string>(StringComparer.Ordinal),
            Id: Guid.NewGuid(), Controllers: Enumerable.Repeat(PeripheralCategory.Joystick, model.ControllerPortCount).ToArray(), Input: new InputConfiguration(), Media: []));
    }

    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId)
    {
        if (configuration is not MachineConfiguration current)
            throw new ArgumentException(nameof(configuration));
        var created = (MachineConfiguration)CreateConfiguration(machineId);
        return created with
        {
            Id = current.Id,
            Options = current.Options,
            AudioEnabled = current.AudioEnabled,
            Input = current.Input,
            Controllers = current.Controllers,
            Audio = current.Audio,
            Media = current.Media
        };
    }

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        if (configuration is not MachineConfiguration current)
            throw new ArgumentException(nameof(configuration));
        var options = new Dictionary<string, string>(current.Options ?? new Dictionary<string, string>(StringComparer.Ordinal), StringComparer.Ordinal);
        CommonCoreHostFunctions.OptionProcessing.ApplyValues(options, values);
        var result = current with { Options = options };
        result = SelectedAdapter(result)?.ApplySettings(result, values) ?? result;
        return result;
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var adapter = SelectedAdapter(current);
        return adapter?.GetRuntimeOptions(current) ?? new Dictionary<string, string>(current.Options ?? new Dictionary<string, string>(StringComparer.Ordinal), StringComparer.Ordinal);
    }

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return ConfigurationSummaryFunctions.Create(current);
    }

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) => InputSettingsFunctions.Describe(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration, EmulationInputSettings settings) => InputSettingsFunctions.Apply(configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration)), settings);

    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => SaveConfigurationAsync(configuration, cancellationToken);

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

    public ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default) =>
        ValueTask.FromException<EmulationMachineRuntime>(new NotSupportedException("The Microsoft machine adapter has not been added yet."));

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false)).Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => configuration is MachineConfiguration current ? SaveValidatedAsync(current, cancellationToken) : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    private ValueTask SaveValidatedAsync(MachineConfiguration configuration, CancellationToken cancellationToken)
    {
        ConfigurationValidationFunctions.ValidateForSave(configuration);
        return new ValueTask(_store.SaveAsync(configuration, cancellationToken));
    }

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(string machineId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = ModelCatalog.Get(machineId);
        return ValueTask.FromException<EmulationEmulatorInstallation>(new NotSupportedException($"No Microsoft emulator adapter is installed for '{machineId}'."));
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return ValueTask.FromResult<IReadOnlyList<EmulationEmulatorInstallation>>([]);
    }

    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(IEmulationConfiguration configuration, string emulatorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        if (_adapters.TryGetValue(emulatorId, out var adapter) && adapter.Definition.MachineIds.Contains(current.Model))
            return ValueTask.FromResult<IEmulationConfiguration>(adapter.SelectConfiguration(current with { EmulatorId = emulatorId }));
        return ValueTask.FromException<IEmulationConfiguration>(new NotSupportedException("No Microsoft emulator adapter is installed."));
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(string machineId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = ModelCatalog.Get(machineId);
        return ValueTask.FromResult<IReadOnlyList<EmulationEmulatorRelease>>([]);
    }

    public ValueTask<string> InstallEmulatorAsync(string machineId, EmulationEmulatorRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = ModelCatalog.Get(machineId);
        return ValueTask.FromException<string>(new NotSupportedException("No Microsoft emulator adapter is installed."));
    }

    public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }
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
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        if (((IEmulationConfiguration)current).MachineId != machineId) throw new ArgumentException(nameof(machineId));
        var directory = GetFirmwareDirectory(machineId);
        if (SelectedAdapter(current) is not { } adapter) return [];
        var candidates = await CommonCoreHostFunctions.FirmwareProcessing.ScanDirectoryAsync(directory,
            (path, token) => adapter.ScanFirmwareFileAsync(current, path, token), cancellationToken).ConfigureAwait(false);
        return candidates.SelectMany(candidateSet => candidateSet).ToArray();
    }

    /// <summary>Revalide le candidat et transmet son rôle au parcours de réglages existant.</summary>
    public IEmulationConfiguration UseFirmware(IEmulationConfiguration configuration, EmulationFirmwareCandidate firmware)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        var adapter = SelectedAdapter(current) ?? throw new ArgumentException(nameof(configuration));
        var scanned = CommonCoreHostFunctions.FirmwareProcessing.SelectCandidate(adapter.ScanFirmwareFileAsync(current, firmware.Path, CancellationToken.None).GetAwaiter().GetResult(), firmware.Id, firmware.DestinationFieldId);
        if (scanned.DestinationFieldId is not { } field || scanned.Compatibility == EmulationFirmwareCompatibility.Incompatible) throw new ArgumentException(nameof(firmware));
        return ApplySettings(current, new Dictionary<string, string?> { [field] = scanned.Path });
    }
}
