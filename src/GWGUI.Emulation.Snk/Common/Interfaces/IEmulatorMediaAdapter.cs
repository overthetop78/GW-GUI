namespace GWGUI.Emulation.Snk.Common.Interfaces;

internal interface IEmulatorMediaAdapter
{
    IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);
    IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration);
}
