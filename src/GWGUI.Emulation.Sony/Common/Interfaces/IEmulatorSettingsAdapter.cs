namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorSettingsAdapter : IEmulatorSettings
{
    IReadOnlyList<EmulationSettingsBlock> GetMachineSettingsBlocks(MachineConfiguration configuration) =>
        SettingsDescriptionFunctions.MachineBlocks(this, configuration);
    IReadOnlyList<EmulationSettingsField> GetStorageDeviceConfigurationFields(MachineConfiguration configuration, EmulationMediaCategory category) =>
        SettingsDescriptionFunctions.StorageFields(this, configuration, category);
}
