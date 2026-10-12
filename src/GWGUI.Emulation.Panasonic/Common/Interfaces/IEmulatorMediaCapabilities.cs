namespace GWGUI.Emulation.Panasonic.Common.Interfaces;

internal interface IEmulatorMediaCapabilities
{
    IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);
}
