using GWGUI.Emulation.Sega.Common.Constants;
using GWGUI.Emulation.Sega.Common.Dictionaries;
using GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Sega.Common.Services;

namespace GWGUI.Emulation.Sega.Modules;

public sealed class SegaEmulationModule : IEmulationModule
{
    private readonly ConfigurationStore _store;

    public SegaEmulationModule(string configurationDirectory)
    {
        _store = new ConfigurationStore(configurationDirectory);
    }

    public string Id => EmulationModuleConstants.ModuleId;
    public string DisplayResourceKey => EmulationModuleConstants.ResourceFamily;
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
        _ = ModelCatalog.Get(machineId);
        return new EmulationMachineSettings(machineId, DefaultVisibility, []);
    }

    public IEmulationConfiguration CreateConfiguration(string machineId)
    {
        var model = ModelCatalog.Get(machineId);
        return new MachineConfiguration(model.Id, EmulatorCatalog.DefaultFor(model.Id),
            Id: Guid.NewGuid(), Media: []);
    }

    public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId)
    {
        if (configuration is not MachineConfiguration current)
            throw new ArgumentException(nameof(configuration));
        var created = (MachineConfiguration)CreateConfiguration(machineId);
        return created with { Id = current.Id, Options = current.Options, Media = current.Media };
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
        var model = ModelCatalog.Get(current.Model);
        return new(model.DisplayResourceKey, [current.EmulatorId]);
    }

    public ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration,
        EmulationRuntimeServices services, CancellationToken cancellationToken = default) =>
        ValueTask.FromException<EmulationMachineRuntime>(new NotSupportedException(
            "The Sega machine adapter has not been added yet."));

    public async ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(
        CancellationToken cancellationToken = default) =>
        (await _store.LoadAllAsync(cancellationToken).ConfigureAwait(false)).Cast<IEmulationConfiguration>().ToArray();

    public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default) => configuration is MachineConfiguration current
        ? new ValueTask(_store.SaveAsync(current, cancellationToken))
        : ValueTask.FromException(new ArgumentException(nameof(configuration)));

    public ValueTask DeleteConfigurationAsync(Guid configurationId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Delete(configurationId);
        return ValueTask.CompletedTask;
    }
}
