namespace GWGUI.Emulation.Sharp.Common.Interfaces;

internal interface IEmulatorStorageSettings
{
    IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);
}
