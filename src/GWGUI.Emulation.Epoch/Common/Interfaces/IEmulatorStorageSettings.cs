namespace GWGUI.Emulation.Epoch.Common.Interfaces;

internal interface IEmulatorStorageSettings
{
    IReadOnlyList<EmulationMediaDevice> DescribeStorage(MachineConfiguration configuration);
}
