using GWGUI.Emulation;
using InputConstants = GWGUI.Emulation.Atari.Emulators.Holani.Constants.InputConstants;

namespace GWGUI.Emulation.Atari.Emulators.Holani.Functions;

internal static class InputFunctions
{
    internal static EmulationInputSnapshot ToNative(EmulationInputSnapshot snapshot) => snapshot with
    {
        Controllers = snapshot.Controllers.Select(state =>
        {
            var buttons = state.Buttons & ~((1u << LynxInputConstants.ButtonACommand)
                | (1u << LynxInputConstants.ButtonBCommand));
            if ((state.Buttons & (1u << LynxInputConstants.ButtonACommand)) != 0)
                buttons |= 1u << InputConstants.OutsideButton;
            if ((state.Buttons & (1u << LynxInputConstants.ButtonBCommand)) != 0)
                buttons |= 1u << InputConstants.InsideButton;
            return state with { Buttons = buttons };
        }).ToArray()
    };
}
