using GWGUI.Emulation;
using GWGUI.Emulation.Atari.Emulators.A5200.Constants;
using InputConstants = GWGUI.Emulation.Atari.Emulators.A5200.Constants.InputConstants;

namespace GWGUI.Emulation.Atari.Emulators.A5200.Functions;

internal static class InputFunctions
{
    internal static EmulationInputSnapshot ToNative(EmulationInputSnapshot snapshot)
    {
        var controllers = snapshot.Controllers.Take(InputConstants.SupportedPortCount).ToArray();
        for (var port = InputConstants.PrimaryPort; port < controllers.Length; port++)
        {
            var state = controllers[port];
            var buttons = state.Buttons & InputConstants.DirectionAndConsoleMask;
            Copy(state.Buttons, ref buttons, InputConstants.Fire1Command, InputConstants.Fire1Button);
            Copy(state.Buttons, ref buttons, InputConstants.Fire2Command, InputConstants.Fire2Button);
            Copy(state.Buttons, ref buttons, InputConstants.HashCommand, InputConstants.HashButton);
            Copy(state.Buttons, ref buttons, InputConstants.StarCommand, InputConstants.StarButton);
            Copy(state.Buttons, ref buttons, InputConstants.Key0Command, InputConstants.Key0Button);
            Copy(state.Buttons, ref buttons, InputConstants.Key1Command, InputConstants.Key1Button);
            Copy(state.Buttons, ref buttons, InputConstants.Key3Command, InputConstants.Key3Button);
            Copy(state.Buttons, ref buttons, InputConstants.Key7Command, InputConstants.Key7Button);
            if (snapshot.Keys.Contains(EmulationKey.AtariStart)) buttons |= 1u << InputConstants.StartButton;
            if (snapshot.Keys.Contains(EmulationKey.AtariSelect)) buttons |= 1u << InputConstants.PauseButton;
            var keypad2 = (state.Buttons & (1u << InputConstants.Key2Command)) != 0;
            controllers[port] = state with
            {
                Buttons = buttons,
                RightX = keypad2 ? InputConstants.KeypadCenterAxis : state.RightX,
                RightY = keypad2 ? InputConstants.KeypadUpAxis : state.RightY
            };
        }
        return snapshot with { Controllers = controllers };
    }

    private static void Copy(uint source, ref uint target, int command, int button)
    {
        if ((source & (1u << command)) != 0) target |= 1u << button;
    }
}
