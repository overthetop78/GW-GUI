namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;

internal static class CoreSettingsConstants
{
    internal const string BlockPrefix = "core-options-";
    internal const int ColumnCount = 2;
    internal static IReadOnlyDictionary<EmulationMachineTab, string> Titles { get; } =
        new Dictionary<EmulationMachineTab, string>
        {
            [EmulationMachineTab.General] = SettingsDescriptionFunctionsConstants.ResourceGeneral,
            [EmulationMachineTab.Cpu] = SettingsDescriptionFunctionsConstants.ResourceCpuProcessor,
            [EmulationMachineTab.Ram] = SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
            [EmulationMachineTab.Video] = SettingsDescriptionFunctionsConstants.ResourceVideo,
            [EmulationMachineTab.Audio] = SettingsDescriptionFunctionsConstants.ResourceAudio,
            [EmulationMachineTab.Storage] = "Emulation.Tab.Storage",
            [EmulationMachineTab.Keyboard] = "Emulation.Tab.Keyboard",
            [EmulationMachineTab.Mouse] = SettingsDescriptionFunctionsConstants.ResourceMouseTab,
            [EmulationMachineTab.Controllers] = "Emulation.Tab.Controllers"
        };
}
