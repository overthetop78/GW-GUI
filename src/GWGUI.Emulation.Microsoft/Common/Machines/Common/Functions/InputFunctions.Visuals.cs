namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(ControllerType type) => type == ControllerType.Joystick ? [EmulationControllerVisualIds.QuickShot, EmulationControllerVisualIds.CompetitionPro5000, EmulationControllerVisualIds.ZipstikSuperPro] : null;

    private static string? DefaultVisualId(ControllerType type) => type == ControllerType.Joystick ? EmulationControllerVisualIds.QuickShot : null;

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(ControllerType type) => type == ControllerType.Joystick
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
        ControllerType.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        ControllerType.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        _ => $"Emulation.Controller.{type}"
    };
}
