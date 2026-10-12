namespace GWGUI.Emulation.Watara.Common.Interfaces;

internal interface IEmulatorMediaCapabilities
{
    IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);
}
