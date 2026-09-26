namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Contracts;

public sealed record MachineConfiguration(
    string Model,
    string EmulatorId,
    IReadOnlyDictionary<string, string>? Options = null,
    Guid Id = default,
    bool AudioEnabled = true,
    IReadOnlyList<ControllerType>? Controllers = null,
    InputConfiguration? Input = null,
    int SchemaVersion = ConfigurationStoreConstants.CurrentSchemaVersion,
    IReadOnlyList<MediaConfiguration>? Media = null,
    AudioConfiguration? Audio = null)
    : GWGUI.Emulation.Interfaces.IEmulationConfiguration
{
    public string ModuleId => MachineConfigurationConstants.ModuleId;
    string GWGUI.Emulation.Interfaces.IEmulationConfiguration.MachineId => Model;
    public MachineConfiguration EnsureId() => Id == Guid.Empty ? this with { Id = Guid.NewGuid() } : this;
}
