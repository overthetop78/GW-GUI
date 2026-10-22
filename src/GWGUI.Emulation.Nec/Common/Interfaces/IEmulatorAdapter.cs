namespace GWGUI.Emulation.Nec.Common.Interfaces;
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorSettings
{
    EmulationStorageSettings DescribeStorage(MachineConfiguration configuration);
    MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings);
}
