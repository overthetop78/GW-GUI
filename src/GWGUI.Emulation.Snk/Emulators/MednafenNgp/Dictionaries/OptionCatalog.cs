using GWGUI.Emulation.Snk.Emulators.MednafenNgp.Constants;

namespace GWGUI.Emulation.Snk.Emulators.MednafenNgp.Dictionaries;

internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> Options { get; } = Array.AsReadOnly(new CoreOption[] { new(RuntimeOptionConstants.Language, "Language", "Language used by games.", EmulationMachineTab.General.ToString(), RuntimeOptionConstants.English, RuntimeOptionConstants.English, [new(RuntimeOptionConstants.English, "English", ResourceKey: SettingsChoiceConstants.ValueEnglish), new(RuntimeOptionConstants.Japanese, "Japanese", ResourceKey: SettingsChoiceConstants.ValueJapanese)], ResourceKey: SettingsTextConstants.SettingLanguage, ExplanationResourceKey: SettingsTextConstants.HelpLanguage) });
}
