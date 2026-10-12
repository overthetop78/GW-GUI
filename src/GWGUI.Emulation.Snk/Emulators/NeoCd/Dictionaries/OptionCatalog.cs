using GWGUI.Emulation.Snk.Emulators.NeoCd.Constants;

namespace GWGUI.Emulation.Snk.Emulators.NeoCd.Dictionaries;

internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> Options { get; } = Array.AsReadOnly(new CoreOption[] { new(
        RuntimeOptionConstants.Region,
        "Console Region",
        null,
        EmulationMachineTab.General.ToString(),
        "Japan",
        "Japan",
        [new("Japan", "Japan", ResourceKey: SettingsChoiceConstants.ValueJapan), new("USA", "USA", ResourceKey: SettingsChoiceConstants.ValueUSA), new("Europe", "Europe", ResourceKey: SettingsChoiceConstants.ValueEurope)],
        ResourceKey: SettingsTextConstants.SettingRegion
    ), new(
        RuntimeOptionConstants.OverscanH,
        "Horizontal Overscan Mask",
        null,
        EmulationMachineTab.Video.ToString(),
        "8",
        "8",
        [
        new("8", "8", ResourceKey: SettingsChoiceConstants.Value8),
        new("4", "4", ResourceKey: SettingsChoiceConstants.Value4),
        new("0", "0", ResourceKey: SettingsChoiceConstants.Value0),
        new("12", "12", ResourceKey: SettingsChoiceConstants.Value12),
        new("16", "16", ResourceKey: SettingsChoiceConstants.Value16)
    ],
        ResourceKey: SettingsTextConstants.SettingHorizontalOverscan
    ), new(RuntimeOptionConstants.Cdspeedhack, "CD Speed Hack", null, EmulationMachineTab.Storage.ToString(), "Off", "Off", [new("On", "On"), new("Off", "Off")], ResourceKey: SettingsTextConstants.SettingCDSpeedHack), new(RuntimeOptionConstants.Loadskip, "Skip CD Loading", null, EmulationMachineTab.Storage.ToString(), "On", "On", [new("On", "On"), new("Off", "Off")], ResourceKey: SettingsTextConstants.SettingSkipCDLoading), new(
        RuntimeOptionConstants.CpuOverclock,
        "CPU Overclock",
        null,
        EmulationMachineTab.Cpu.ToString(),
        "100%",
        "100%",
        [
        new("100%", "100%", ResourceKey: SettingsChoiceConstants.Value100Percent),
        new("110%", "110%", ResourceKey: SettingsChoiceConstants.Value110Percent),
        new("125%", "125%", ResourceKey: SettingsChoiceConstants.Value125Percent),
        new("150%", "150%", ResourceKey: SettingsChoiceConstants.Value150Percent),
        new("200%", "200%", ResourceKey: SettingsChoiceConstants.Value200Percent)
    ],
        ResourceKey: SettingsTextConstants.SettingCPUClock
    ), new(RuntimeOptionConstants.PerContentSaves, "Per-Game Saves", null, EmulationMachineTab.Storage.ToString(), "Off", "Off", [new("On", "On"), new("Off", "Off")], ResourceKey: SettingsTextConstants.SettingPerGameSaves), new(
        RuntimeOptionConstants.AspectRatio,
        "Aspect Ratio",
        null,
        EmulationMachineTab.Video.ToString(),
        "1:1 PAR",
        "1:1 PAR",
        [
        new("1:1 PAR", "1:1 PAR", ResourceKey: SettingsChoiceConstants.ValueRatio11PAR),
        new("45:44 PAR", "45:44 PAR", ResourceKey: SettingsChoiceConstants.ValueRatio4544PAR),
        new("4:3 DAR", "4:3 DAR", ResourceKey: SettingsChoiceConstants.ValueRatio43DAR)
    ],
        ResourceKey: SettingsTextConstants.SettingAspectRatio
    ), });
}
