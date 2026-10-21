namespace GWGUI.Emulation.Amstrad.Common.Interfaces;
internal interface IEmulatorAdapter : global::GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>
{
    IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration, IReadOnlyList<CoreOption> coreOptions);
    IReadOnlyList<CoreOption> GetOptions(EmulatorManagementContext context);
}
