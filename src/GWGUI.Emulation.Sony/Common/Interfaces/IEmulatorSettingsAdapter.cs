namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorSettingsAdapter
{
    IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(MachineConfiguration configuration);
    IReadOnlyList<EmulationSettingsBlock> GetMachineSettingsBlocks(MachineConfiguration configuration) =>
        SettingsDescriptionFunctions.MachineBlocks(this, configuration);
    IReadOnlyList<EmulationSettingsField> GetStorageDeviceConfigurationFields(MachineConfiguration configuration, EmulationMediaCategory category) =>
        SettingsDescriptionFunctions.StorageFields(this, configuration, category);
    void ApplySettingsOptions(Dictionary<string, string> options, IReadOnlyDictionary<string, string?> values) { }
    void ValidateConfiguration(MachineConfiguration configuration) { }
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration) => configuration.Options ?? new Dictionary<string, string>();
    int GetMemoryKib(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).RamKib;
    IReadOnlyList<int> GetAudioLatencyChoices(MachineConfiguration configuration) => AudioConstants.LatencyMilliseconds;
    AudioConfiguration GetAudioConfiguration(MachineConfiguration configuration) => configuration.Audio ?? new AudioConfiguration();
    MachineConfiguration ApplyVideoSettings(MachineConfiguration configuration, EmulationGraphicsApi graphicsApi) => configuration with { GraphicsApi = graphicsApi };
    bool IsCpuClock(string fieldId) => false;
}
