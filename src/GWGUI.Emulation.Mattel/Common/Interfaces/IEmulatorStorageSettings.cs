namespace GWGUI.Emulation.Mattel.Common.Interfaces;

internal interface IEmulatorStorageSettings
{
    IReadOnlyList<EmulationMediaDevice> DescribeStorage(MachineConfiguration configuration);
}
