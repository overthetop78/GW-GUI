using GWGUI.Emulation.Microsoft.Common.Constants;
using GWGUI.Emulation.Microsoft.Common.Dictionaries;
using GWGUI.Emulation.Microsoft.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Microsoft.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Microsoft.Common.Services;

namespace GWGUI.Emulation.Microsoft.Modules;

public sealed class MicrosoftEmulationModule : IEmulationModule, IEmulationEmulatorManager,
    IEmulationInputSettingsManager, IEmulationStorageSettingsManager,
    IEmulationModuleLocalization
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(MicrosoftEmulationModule).Assembly, "GWGUI.Emulation.Microsoft.Resources.Emulation");
    private readonly ConfigurationStore _store;

    public bool TryGetString(string key, System.Globalization.CultureInfo culture, out string value) =>
        Localization.TryGetString(key, culture, out value);

    public MicrosoftEmulationModule(string configurationDirectory, string pathBase,
        HttpClient httpClient, string coreDirectory)
    {
        _store = new ConfigurationStore(configurationDirectory, pathBase);
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
    public string BrandImageResourceName =>
        $"{EmulationModuleConstants.AssetResourcePrefix}.microsoft.png";
    public IReadOnlyList<EmulationMachineDefinition> Machines => MachineCatalog.All;
    public EmulationSettingsVisibility DefaultVisibility { get; } = new(
        Enum.GetValues<EmulationMachineTab>().ToDictionary(tab => tab, _ => true));

    public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        exitCode = 0;
        return false;
    }

    public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null)
    {
        var model = ModelCatalog.Get(machineId);
        var current = configuration as MachineConfiguration
            ?? (MachineConfiguration)CreateConfiguration(machineId);
        var tabs = DefaultVisibility.Tabs.ToDictionary(item => item.Key, item => item.Key switch
        {
            EmulationMachineTab.Keyboard => model.HasKeyboard,
            EmulationMachineTab.Mouse => model.MouseButtonCount > 0,
            EmulationMachineTab.Storage => model.MaximumFloppyDriveCount > 0
                || model.SupportsCassetteDrive || model.SupportsCartridgeSlot
                || model.SupportsCompactDiscDrive,
            _ => item.Value
        });
        return new EmulationMachineSettings(machineId, new EmulationSettingsVisibility(tabs),
            SettingsDescriptionFunctions.Create(current));
    }

    public IEmulationConfiguration CreateConfiguration(string machineId)
    {
        var model = ModelCatalog.Get(machineId);
        return new MachineConfiguration(model.Id, EmulatorCatalog.GetAll(model.Id)
                .FirstOrDefault()?.Id ?? string.Empty,
            Options: new Dictionary<string, string>(StringComparer.Ordinal),
            Id: Guid.NewGuid(), Controllers: Enumerable.Repeat(ControllerType.Joystick,
                model.ControllerPortCount).ToArray(), Input: new InputConfiguration(), Media: []);
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

    public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration,
        IReadOnlyDictionary<string, string?> values)
    {
        if (configuration is not MachineConfiguration current)
            throw new ArgumentException(nameof(configuration));
        var options = new Dictionary<string, string>(
            current.Options ?? new Dictionary<string, string>(StringComparer.Ordinal),
            StringComparer.Ordinal);
        foreach (var value in values)
        {
            if (value.Value is null) options.Remove(value.Key);
            else options[value.Key] = value.Value;
        }
        return current with { Options = options };
    }

    public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration) =>
        new Dictionary<string, string>((configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration))).Options
            ?? new Dictionary<string, string>(StringComparer.Ordinal), StringComparer.Ordinal);

    public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration)
    {
        var current = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return ConfigurationSummaryFunctions.Create(current);
    }

    public EmulationInputSettings DescribeInputSettings(IEmulationConfiguration configuration) =>
        InputSettingsFunctions.Describe(configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyInputSettings(IEmulationConfiguration configuration,
        EmulationInputSettings settings) => InputSettingsFunctions.Apply(configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)), settings);

    public ValueTask SaveInputSettingsAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => SaveConfigurationAsync(configuration, cancellationToken);

    public EmulationStorageSettings DescribeStorageSettings(IEmulationConfiguration configuration) =>
        StorageSettingsFunctions.Describe(configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)));

    public IEmulationConfiguration ApplyStorageSettings(IEmulationConfiguration configuration,
        EmulationStorageSettings settings) => StorageSettingsFunctions.Apply(configuration as MachineConfiguration
            ?? throw new ArgumentException(nameof(configuration)), settings);

    public ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration,
        EmulationRuntimeServices services, CancellationToken cancellationToken = default) =>
        ValueTask.FromException<EmulationMachineRuntime>(new NotSupportedException(
            "The Microsoft machine adapter has not been added yet."));

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(
        CancellationToken cancellationToken = default) =>
        (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false)).Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is MachineConfiguration current
        ? SaveValidatedAsync(current, cancellationToken)
        : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    private ValueTask SaveValidatedAsync(MachineConfiguration configuration,
        CancellationToken cancellationToken)
    {
        ConfigurationValidationFunctions.ValidateForSave(configuration);
        return new ValueTask(_store.SaveAsync(configuration, cancellationToken));
    }

    public ValueTask<EmulationEmulatorInstallation> GetEmulatorInstallationAsync(
        string machineId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = ModelCatalog.Get(machineId);
        return ValueTask.FromException<EmulationEmulatorInstallation>(
            new NotSupportedException($"No Microsoft emulator adapter is installed for '{machineId}'."));
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorInstallation>> GetEmulatorInstallationsAsync(
        IEmulationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return ValueTask.FromResult<IReadOnlyList<EmulationEmulatorInstallation>>([]);
    }

    public ValueTask<IEmulationConfiguration> UseEmulatorAsync(
        IEmulationConfiguration configuration, string emulatorId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = configuration as MachineConfiguration ?? throw new ArgumentException(nameof(configuration));
        return ValueTask.FromException<IEmulationConfiguration>(
            new NotSupportedException("No Microsoft emulator adapter is installed."));
    }

    public ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindEmulatorReleasesAsync(
        string machineId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = ModelCatalog.Get(machineId);
        return ValueTask.FromResult<IReadOnlyList<EmulationEmulatorRelease>>([]);
    }

    public ValueTask<string> InstallEmulatorAsync(string machineId,
        EmulationEmulatorRelease release, IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = ModelCatalog.Get(machineId);
        return ValueTask.FromException<string>(
            new NotSupportedException("No Microsoft emulator adapter is installed."));
    }

    public ValueTask DeleteConfigurationAsync(Guid configurationId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }
}
