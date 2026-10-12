namespace GWGUI.Emulation.Thomson.Common.Interfaces;

internal interface IEmulatorMediaCapabilities
{
    IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);
}
