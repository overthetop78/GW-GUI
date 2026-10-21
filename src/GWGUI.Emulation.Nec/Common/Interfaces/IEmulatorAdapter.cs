namespace GWGUI.Emulation.Nec.Common.Interfaces;
internal interface IEmulatorAdapter : global::GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>
{
    IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration);
    EmulationStorageSettings DescribeStorage(MachineConfiguration configuration);
    MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings);
}
