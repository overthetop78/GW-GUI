namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(PeripheralCategory type) => type switch
    {
        PeripheralCategory.Joystick => [EmulationControllerVisualIds.QuickShot, EmulationControllerVisualIds.CompetitionPro5000, EmulationControllerVisualIds.ZipstikSuperPro],
        PeripheralCategory.Gamepad => [EmulationControllerVisualIds.AmstradGx4000Controller],
        _ => null
    };

    private static string? DefaultVisualId(PeripheralCategory type) => type switch
    {
        PeripheralCategory.Joystick => EmulationControllerVisualIds.QuickShot,
        PeripheralCategory.Gamepad => EmulationControllerVisualIds.AmstradGx4000Controller,
        _ => null
    };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(PeripheralCategory type) => type is PeripheralCategory.Joystick or PeripheralCategory.Gamepad
        ? new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A
        } : null;

    private static string ControllerResourceKey(PeripheralCategory type) => type switch
    {
        PeripheralCategory.Joystick => InputSettingsFunctionsConstants.ResourceControllerJoystick,
        PeripheralCategory.Gamepad => InputSettingsFunctionsConstants.ResourceControllerGamepad,
        PeripheralCategory.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        PeripheralCategory.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        _ => $"{InputSettingsFunctionsConstants.EmulationController}{type}"
    };
}
