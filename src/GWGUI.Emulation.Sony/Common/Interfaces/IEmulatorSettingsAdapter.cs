namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorSettingsAdapter
{
    IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration);
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options ?? new Dictionary<string, string>();
    int GetMemoryKib(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).RamKib;
    bool IsCpuClock(string fieldId) => false;
}
