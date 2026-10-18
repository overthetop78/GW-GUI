namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(ControllerType type) => type switch
    {
        ControllerType.Joystick => [EmulationControllerVisualIds.QuickShot, EmulationControllerVisualIds.CompetitionPro5000, EmulationControllerVisualIds.ZipstikSuperPro],
        ControllerType.Gamepad => [EmulationControllerVisualIds.AmstradGx4000Controller],
        _ => null
    };

    private static string? DefaultVisualId(ControllerType type) => type switch
    {
        ControllerType.Joystick => EmulationControllerVisualIds.QuickShot,
        ControllerType.Gamepad => EmulationControllerVisualIds.AmstradGx4000Controller,
        _ => null
    };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(ControllerType type) => type is ControllerType.Joystick or ControllerType.Gamepad
        ? new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A
        } : null;

    private static string ControllerResourceKey(ControllerType type) => type switch
    {
        ControllerType.Joystick => InputSettingsFunctionsConstants.ResourceControllerJoystick,
        ControllerType.Gamepad => InputSettingsFunctionsConstants.ResourceControllerGamepad,
        ControllerType.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        ControllerType.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        _ => $"{InputSettingsFunctionsConstants.EmulationController}{type}"
    };
}
