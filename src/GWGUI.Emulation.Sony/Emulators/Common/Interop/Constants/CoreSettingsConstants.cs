namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

internal static class CoreSettingsConstants
{
    internal const string OptionsBlock = "emulatorOptions";
    internal const string OptionResourcePrefix = "Emulation.Option.";
    internal const string NumericFormat = "0.########";
    internal const string StorageTitleResource = "Emulation.Tab.Storage";
    internal const string MouseTitleResource = "Emulation.Tab.Mouse";
    internal const string ControllersTitleResource = "Emulation.Tab.Controllers";
    internal const string StorageIcon = "\uEDA2";
    internal const string MouseIcon = "\uE962";
    internal const string ControllersIcon = "\uE7FC";
    internal static IReadOnlyDictionary<EmulationMachineTab, (string Title, string Icon)> Sections { get; } =
        new Dictionary<EmulationMachineTab, (string, string)>
        {
            [EmulationMachineTab.General] = (SettingsDescriptionFunctionsConstants.ResourceGeneral, SettingsDescriptionFunctionsConstants.IconGeneral),
            [EmulationMachineTab.Cpu] = (SettingsDescriptionFunctionsConstants.ResourceCpuProcessor, SettingsDescriptionFunctionsConstants.IconCpu),
            [EmulationMachineTab.Ram] = (SettingsDescriptionFunctionsConstants.ResourceMemoryMain, SettingsDescriptionFunctionsConstants.IconMemory),
            [EmulationMachineTab.Rom] = (SettingsDescriptionFunctionsConstants.ResourceRom, SettingsDescriptionFunctionsConstants.IconFirmware),
            [EmulationMachineTab.Video] = (SettingsDescriptionFunctionsConstants.ResourceVideo, SettingsDescriptionFunctionsConstants.IconVideo),
            [EmulationMachineTab.Audio] = (SettingsDescriptionFunctionsConstants.ResourceAudio, SettingsDescriptionFunctionsConstants.IconAudio),
            [EmulationMachineTab.Storage] = (StorageTitleResource, StorageIcon),
            [EmulationMachineTab.Mouse] = (MouseTitleResource, MouseIcon),
            [EmulationMachineTab.Controllers] = (ControllersTitleResource, ControllersIcon)
        };
}
