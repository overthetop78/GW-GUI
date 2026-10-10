using GWGUI.Emulation.Mattel.Emulators.FreeIntv.Constants;

namespace GWGUI.Emulation.Mattel.Emulators.FreeIntv.Dictionaries;

internal static class FreeIntvOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(OptionConstants.PlayerOneController, "Default Player 1 Controller (Restart)",
            "Choose the console controller used by player one.", null,
            OptionConstants.Right, OptionConstants.Right,
            [new(OptionConstants.Right, "Right controller"), new(OptionConstants.Left, "Left controller")]),
        new(OptionConstants.KeypadOverlay, "Onscreen Interactive Keypad Overlays (Restart and Touchscreen/Mouse Required)",
            "Display the native interactive keypad workspace.", null,
            OptionConstants.Disabled, OptionConstants.Disabled,
            [new(OptionConstants.Disabled, "Disabled"), new(OptionConstants.Enabled, "Enabled")])
    ];
}
