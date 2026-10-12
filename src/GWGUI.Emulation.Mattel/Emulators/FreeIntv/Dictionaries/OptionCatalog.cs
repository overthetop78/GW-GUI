using GWGUI.Emulation.Mattel.Emulators.FreeIntv.Constants;

namespace GWGUI.Emulation.Mattel.Emulators.FreeIntv.Dictionaries;

internal static class FreeIntvOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(OptionConstants.PlayerOneController, "Default Player 1 Controller (Restart)",
            "Choose the console controller used by player one.", null,
            OptionConstants.Right, OptionConstants.Right,
            [new(OptionConstants.Right, "Right controller", ResourceKey: SettingsChoiceConstants.RightController), new(OptionConstants.Left, "Left controller", ResourceKey: SettingsChoiceConstants.LeftController)],
            ResourceKey: SettingsTextConstants.PlayerOneController, ExplanationResourceKey: SettingsTextConstants.PlayerOneControllerHelp),
        new(OptionConstants.KeypadOverlay, "Onscreen Interactive Keypad Overlays (Restart and Touchscreen/Mouse Required)",
            "Display the native interactive keypad workspace.", null,
            OptionConstants.Disabled, OptionConstants.Disabled,
            [new(OptionConstants.Disabled, "Disabled", ResourceKey: SettingsChoiceConstants.Disabled), new(OptionConstants.Enabled, "Enabled", ResourceKey: SettingsChoiceConstants.Enabled)], IsVisible: false)
    ];
}
