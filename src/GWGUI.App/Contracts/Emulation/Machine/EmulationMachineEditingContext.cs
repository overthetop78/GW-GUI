namespace GWGUI.App.Contracts.Emulation.Machine;

internal sealed record EmulationMachineEditingContext(string ModuleDisplayName, string MachineDisplayName) : EventArgs;
