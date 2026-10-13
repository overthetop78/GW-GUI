namespace GWGUI.Emulation.Acorn.Common.Interfaces;

internal interface IEmulatorStorageSettings
{
    IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);
}
