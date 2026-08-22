namespace GWGUI.Emulation;

public interface IEmulationModule
{
    string Id { get; }
    string DisplayResourceKey { get; }
    IReadOnlyList<EmulationMachineDefinition> Machines { get; }
    EmulationSettingsVisibility DefaultVisibility { get; }
    bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode);

    EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null);
    IEmulationConfiguration CreateConfiguration(string machineId);
    IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId);
    IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration,
        IReadOnlyDictionary<string, string?> values);
    EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration);
    ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration,
        EmulationRuntimeServices services, CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(
        CancellationToken cancellationToken = default);
    ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration,
        CancellationToken cancellationToken = default);
    ValueTask DeleteConfigurationAsync(Guid configurationId,
        CancellationToken cancellationToken = default);
}
