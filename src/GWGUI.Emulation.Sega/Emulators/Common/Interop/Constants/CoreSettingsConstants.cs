namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Constants;

internal static class CoreSettingsConstants
{
    internal const string TransportName = "Sega";
    internal const string SearchPattern = "*";
    internal const string FirmwareFolder = "firmware";
    internal const string OptionsBlock = "emulatorOptions";
    internal const string OptionResourcePrefix = "Emulation.Option.";
    internal const int OptionsColumnCount = 1;
    internal static IReadOnlyDictionary<EmulationMachineTab, (string Title, string Icon)> Sections { get; } =
        new Dictionary<EmulationMachineTab, (string, string)>
        {
            [EmulationMachineTab.General] = (SettingsDescriptionFunctionsConstants.ResourceGeneral, SettingsDescriptionFunctionsConstants.IconGeneral),
            [EmulationMachineTab.Cpu] = (SettingsDescriptionFunctionsConstants.ResourceCpuProcessor, SettingsDescriptionFunctionsConstants.IconCpu),
            [EmulationMachineTab.Ram] = (SettingsDescriptionFunctionsConstants.ResourceMemoryMain, SettingsDescriptionFunctionsConstants.IconMemory),
            [EmulationMachineTab.Rom] = (SettingsDescriptionFunctionsConstants.ResourceRom, SettingsDescriptionFunctionsConstants.IconFirmware),
            [EmulationMachineTab.Video] = (SettingsDescriptionFunctionsConstants.ResourceVideo, SettingsDescriptionFunctionsConstants.IconVideo),
            [EmulationMachineTab.Audio] = (SettingsDescriptionFunctionsConstants.ResourceAudio, SettingsDescriptionFunctionsConstants.IconAudio),
            [EmulationMachineTab.Storage] = ("Emulation.Tab.Storage", "\uEDA2"),
            [EmulationMachineTab.Keyboard] = ("Emulation.Tab.Keyboard", "\uE765"),
            [EmulationMachineTab.Mouse] = ("Emulation.Tab.Mouse", "\uE962"),
            [EmulationMachineTab.Controllers] = ("Emulation.Tab.Controllers", "\uE7FC"),
        };
}
