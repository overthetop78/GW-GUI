namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorSettingsAdapter
{
    IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration);
    IReadOnlyList<EmulationSettingsBlock> GetMachineSettingsBlocks(MachineConfiguration configuration) => GetSettingsBlocks(configuration).Where(block => block.Tab != EmulationMachineTab.Storage).ToArray();
    IReadOnlyList<EmulationSettingsField> GetStorageDeviceConfigurationFields(MachineConfiguration configuration, EmulationMediaCategory category) =>
        category == EmulationMediaCategory.CompactDiscDrive ? GetSettingsBlocks(configuration).Where(block => block.Tab == EmulationMachineTab.Storage).SelectMany(block => block.Fields).ToArray() : [];
    void ValidateConfiguration(MachineConfiguration configuration) { }
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options ?? new Dictionary<string, string>();
    int GetMemoryKib(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).RamKib;
    IReadOnlyList<int> GetAudioLatencyChoices(MachineConfiguration configuration) => AudioConstants.LatencyMilliseconds;
    AudioConfiguration GetAudioConfiguration(MachineConfiguration configuration) => configuration.Audio ?? new AudioConfiguration();
    MachineConfiguration ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi) => configuration with { GraphicsApi = graphicsApi };
    bool IsCpuClock(string fieldId) => false;
}
