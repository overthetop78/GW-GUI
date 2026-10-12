namespace GWGUI.Emulation.Philips.Common.Interfaces;

internal interface IEmulatorStorageSettings
{
    IReadOnlyList<EmulationMediaDevice> DescribeStorage(MachineConfiguration configuration);
}
