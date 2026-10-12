using GWGUI.Emulation.Watara.Emulators.Potator.Constants;
using GWGUI.Emulation.Watara.Emulators.Potator.Dictionaries;
using GWGUI.Emulation.Watara.Emulators.Potator.Enums;

namespace GWGUI.Emulation.Watara.Emulators.Potator.Functions;
internal static class SettingsDescriptionFunctions
{
    private const string Block = "Emulation.Watara.Video";
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration) => [new(
            Block,
            EmulationMachineTab.Video,
            "Emulation.Video.Settings.Display",
            [
                Field(configuration, RuntimeOptionConstants.Palette, "Emulation.Watara.Video.Palette", RuntimeOptionConstants.DefaultPalette, SettingsDictionary.Palettes),
                Field(configuration, RuntimeOptionConstants.LcdGhosting, "Emulation.Watara.Video.Ghosting", RuntimeOptionConstants.DefaultGhosting, SettingsDictionary.Ghosting),
                Field(configuration, RuntimeOptionConstants.FrameSkip, "Emulation.Video.FrameSkip", SettingsDictionary.FrameSkipValues[FrameSkipMode.Disabled], SettingsDictionary.FrameSkip),
                Field(configuration, RuntimeOptionConstants.FrameSkipThreshold, "Emulation.Watara.Video.Threshold", RuntimeOptionConstants.DefaultThreshold, SettingsDictionary.Thresholds)with { IsEnabled = configuration.Options.GetValueOrDefault(RuntimeOptionConstants.FrameSkip) == SettingsDictionary.FrameSkipValues[FrameSkipMode.Manual] }
            ],
            Columns: 2
        )];
    private static EmulationSettingsField Field(MachineConfiguration configuration, string key, string label, string defaultValue, IReadOnlyList<EmulationSettingsChoice> choices) => new(key, EmulationMachineTab.Video, Block, label, EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(key, defaultValue), choices, RefreshSettingsOnChange: key == RuntimeOptionConstants.FrameSkip);
}