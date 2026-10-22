namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(PeripheralCategory type) => type == PeripheralCategory.Joystick ? [EmulationControllerVisualIds.QuickShot, EmulationControllerVisualIds.CompetitionPro5000, EmulationControllerVisualIds.ZipstikSuperPro] : null;

    private static string? DefaultVisualId(PeripheralCategory type) => type == PeripheralCategory.Joystick ? EmulationControllerVisualIds.QuickShot : null;

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(PeripheralCategory type) => type == PeripheralCategory.Joystick
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
        PeripheralCategory.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        PeripheralCategory.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        _ => $"Emulation.Controller.{type}"
    };
}
