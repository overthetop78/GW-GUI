using GWGUI.Emulation;

namespace GWGUI.App.Contracts;

internal readonly record struct EmulationMachineTabDefinition(
    EmulationMachineTab Tab,
    string Icon,
    string ResourceKey);
