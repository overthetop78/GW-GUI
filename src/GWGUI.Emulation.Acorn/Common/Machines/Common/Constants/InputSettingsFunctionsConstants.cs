namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Constants;

internal static class InputSettingsFunctionsConstants
{
    internal const string ResourceControllerNone = "Emulation.Controller.None";
    internal const string ResourceControllerAutomatic = "Emulation.Controller.Automatic";
    internal const string ResourceControllerAnalogJoystick = "Emulation.Controller.AnalogJoystick";
    internal const string ResourceActionPrefix = "Emulation.Controller.Action.";

    internal static readonly IReadOnlyList<string> ControllerActions =
    [
        EmulationControllerCommandIds.Up, EmulationControllerCommandIds.Down,
        EmulationControllerCommandIds.Left, EmulationControllerCommandIds.Right,
        EmulationControllerCommandIds.Fire1
    ];
}
