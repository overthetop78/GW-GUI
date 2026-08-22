using GWGUI.Emulation;

namespace GWGUI.App.Contracts.Emulation.Machine;

internal readonly record struct EmulationMachineTabDefinition(
    EmulationMachineTab Tab,
    string Icon,
    string ResourceKey);
