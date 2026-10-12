namespace GWGUI.Emulation.Fairchild.Common.Interfaces;

internal interface IEmulatorStorageSettings
{
    IReadOnlyList<EmulationMediaDevice> DescribeStorage(MachineConfiguration configuration);
}
