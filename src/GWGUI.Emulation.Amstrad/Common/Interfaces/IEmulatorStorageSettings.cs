namespace GWGUI.Emulation.Amstrad.Common.Interfaces;

internal interface IEmulatorStorageSettings
{
    EmulationStorageSettings DescribeStorage(MachineConfiguration configuration);
    MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings);
}
