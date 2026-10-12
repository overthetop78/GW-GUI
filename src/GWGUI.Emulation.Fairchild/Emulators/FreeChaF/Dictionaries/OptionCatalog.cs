using GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Constants;

namespace GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Dictionaries;

internal static class FreeChaFOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } = [new(
        OptionConstants.FastScreenClear,
        "Clear screen in single frame",
        null,
        null,
        OptionConstants.Disabled,
        OptionConstants.Disabled,
        [new(OptionConstants.Disabled, OptionConstants.Disabled, ResourceKey: SettingsChoiceConstants.Disabled), new(OptionConstants.Enabled, OptionConstants.Enabled, ResourceKey: SettingsChoiceConstants.Enabled)],
        ResourceKey: SettingsTextConstants.FastScreenClear,
        ExplanationResourceKey: SettingsTextConstants.FastScreenClearHelp
    )];
}
