namespace GWGUI.App.Contracts.Emulation.Machine;

internal sealed class EmulationMachineEditingContext : EventArgs
{
    internal EmulationMachineEditingContext(string moduleDisplayName, string machineDisplayName)
    {
        ModuleDisplayName = moduleDisplayName;
        MachineDisplayName = machineDisplayName;
    }

    internal string ModuleDisplayName { get; }
    internal string MachineDisplayName { get; }
}
