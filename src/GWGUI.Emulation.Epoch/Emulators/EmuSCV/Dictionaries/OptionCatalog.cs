using GWGUI.Emulation.Epoch.Emulators.EmuSCV.Constants;

namespace GWGUI.Emulation.Epoch.Emulators.EmuSCV.Dictionaries;

internal static class EmuSCVOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(OptionConstants.OptionConsole, "CONSOLE", null, null, "AUTO", "AUTO",
            [
                new("AUTO", "AUTO", ResourceKey: SettingsChoiceConstants.Automatic),
                new("EPOCH", "EPOCH", InvariantDisplayValue: "Epoch"),
                new("YENO", "YENO", InvariantDisplayValue: "Yeno"),
                new("EPOCHLADY", "EPOCHLADY", InvariantDisplayValue: "Epoch Lady")
            ], IsVisible: false),
        new(OptionConstants.OptionDisplay, "DISPLAY", null, null, "AUTO", "AUTO",
            [
                new("AUTO", "AUTO", ResourceKey: SettingsChoiceConstants.Automatic),
                new("EMUSCV", "EMUSCV", InvariantDisplayValue: "EmuSCV"),
                new("EPOCH", "EPOCH", InvariantDisplayValue: "Epoch"),
                new("YENO", "YENO", InvariantDisplayValue: "Yeno")
            ], ResourceKey: SettingsTextConstants.DisplayArea, ExplanationResourceKey: SettingsTextConstants.DisplayAreaHelp),
        new(OptionConstants.OptionPixelaspect, "PIXELASPECT", null, null, "AUTO", "AUTO",
            [
                new("AUTO", "AUTO", ResourceKey: SettingsChoiceConstants.Automatic),
                new("RECTANGULAR", "RECTANGULAR", ResourceKey: SettingsChoiceConstants.Rectangular),
                new("SQUARE", "SQUARE", ResourceKey: SettingsChoiceConstants.Square)
            ], ResourceKey: SettingsTextConstants.PixelAspect, ExplanationResourceKey: SettingsTextConstants.PixelAspectHelp),
        new(OptionConstants.OptionResolution, "RESOLUTION", null, null, "AUTO", "AUTO",
            [
                new("AUTO", "AUTO", ResourceKey: SettingsChoiceConstants.Automatic),
                new("LOW", "LOW", ResourceKey: SettingsChoiceConstants.Low),
                new("MEDIUM", "MEDIUM", ResourceKey: SettingsChoiceConstants.Medium),
                new("HIGH", "HIGH", ResourceKey: SettingsChoiceConstants.High)
            ], ResourceKey: SettingsTextConstants.Resolution, ExplanationResourceKey: SettingsTextConstants.ResolutionHelp),
        new(OptionConstants.OptionPalette, "PALETTE", null, null, "AUTO", "AUTO",
            [
                new("AUTO", "AUTO", ResourceKey: SettingsChoiceConstants.Automatic),
                new("STANDARD", "STANDARD", ResourceKey: SettingsChoiceConstants.Standard),
                new("OLDNTSC", "OLDNTSC", InvariantDisplayValue: "OLDNTSC")
            ], ResourceKey: SettingsTextConstants.Palette, ExplanationResourceKey: SettingsTextConstants.PaletteHelp),
        new(OptionConstants.OptionFps, "FPS", null, null, "AUTO", "AUTO",
            [
                new("AUTO", "AUTO", ResourceKey: SettingsChoiceConstants.Automatic),
                new("EPOCH60", "EPOCH60", FrequencyHz: 60),
                new("YENO50", "YENO50", FrequencyHz: 50)
            ], ResourceKey: SettingsTextConstants.RefreshRate, ExplanationResourceKey: SettingsTextConstants.RefreshRateHelp),
        new(OptionConstants.OptionDisplayfullmemory, "DISPLAYFULLMEMORY", null, null, "AUTO", "AUTO",
            [
                new("AUTO", "AUTO", ResourceKey: SettingsChoiceConstants.Automatic),
                new("YES", "YES", ResourceKey: SettingsChoiceConstants.Yes),
                new("NO", "NO", ResourceKey: SettingsChoiceConstants.No)
            ], ResourceKey: SettingsTextConstants.ShowVideoMemory, ExplanationResourceKey: SettingsTextConstants.ShowVideoMemoryHelp),
        new(OptionConstants.OptionDisplayinputs, "DISPLAYINPUTS", null, null, "AUTO", "AUTO",
            [
                new("AUTO", "AUTO", ResourceKey: SettingsChoiceConstants.Automatic),
                new("YES", "YES", ResourceKey: SettingsChoiceConstants.Yes),
                new("NO", "NO", ResourceKey: SettingsChoiceConstants.No)
            ], ResourceKey: SettingsTextConstants.ShowInputs, ExplanationResourceKey: SettingsTextConstants.ShowInputsHelp),
        new(OptionConstants.OptionLanguage, "LANGUAGE", null, null, "AUTO", "AUTO",
            [
                new("AUTO", "AUTO", ResourceKey: SettingsChoiceConstants.Automatic),
                new("JP", "JP", InvariantDisplayValue: "JP"),
                new("FR", "FR", InvariantDisplayValue: "FR"),
                new("EN", "EN", InvariantDisplayValue: "EN")
            ], ResourceKey: SettingsTextConstants.Language, ExplanationResourceKey: SettingsTextConstants.LanguageHelp),
        new(OptionConstants.OptionCheckbios, "CHECKBIOS", null, null, "AUTO", "AUTO",
            [
                new("AUTO", "AUTO", ResourceKey: SettingsChoiceConstants.Automatic),
                new("YES", "YES", ResourceKey: SettingsChoiceConstants.Yes),
                new("NO", "NO", ResourceKey: SettingsChoiceConstants.No)
            ], ResourceKey: SettingsTextConstants.ValidateFirmware, ExplanationResourceKey: SettingsTextConstants.ValidateFirmwareHelp),
        new(OptionConstants.OptionKeyboardatstart, "KEYBOARDATSTART", null, null, "AUTO", "AUTO",
            [
                new("AUTO", "AUTO", ResourceKey: SettingsChoiceConstants.Automatic),
                new("YES", "YES", ResourceKey: SettingsChoiceConstants.Yes),
                new("NO", "NO", ResourceKey: SettingsChoiceConstants.No)
            ], ResourceKey: SettingsTextConstants.ShowKeypadAtStart, ExplanationResourceKey: SettingsTextConstants.ShowKeypadAtStartHelp),
    ];
}
