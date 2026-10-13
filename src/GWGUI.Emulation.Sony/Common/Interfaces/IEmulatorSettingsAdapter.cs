namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorSettingsAdapter
{
    IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration);
    int GetMemoryKib(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).RamKib;
    bool IsCpuClock(string fieldId) => false;
}
