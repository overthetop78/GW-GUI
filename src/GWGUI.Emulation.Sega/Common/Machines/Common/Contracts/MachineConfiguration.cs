using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Sega.Common.Machines.Common.Constants;

namespace GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;

public sealed record MachineConfiguration(
    string Model,
    string EmulatorId,
    IReadOnlyDictionary<string, string>? Options = null,
    Guid Id = default,
    IReadOnlyList<EmulationMedia>? Media = null,
    int SchemaVersion = MachineConfigurationConstants.CurrentSchemaVersion)
    : IEmulationConfiguration
{
    public string ModuleId => MachineConfigurationConstants.ModuleId;
    string IEmulationConfiguration.MachineId => Model;
    public MachineConfiguration EnsureId() => Id == Guid.Empty ? this with { Id = Guid.NewGuid() } : this;
}
