namespace GWGUI.Emulation.Philips.Common.Machines.Common.Dictionaries;

internal static class ControllerVisualDictionary
{
    internal static string? VisualId(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.Joystick => EmulationControllerVisualIds.PhilipsVideopacJoystick,
        PeripheralCategory.CdiGamepad => EmulationControllerVisualIds.PhilipsCdiGamepad,
        PeripheralCategory.Mouse => EmulationControllerVisualIds.PhilipsCdiMouse,
        _ => null
    };

    internal static IReadOnlyDictionary<EmulationControllerVisualControl, string> Commands(PeripheralCategory peripheral)
    {
        var commands = new Dictionary<EmulationControllerVisualControl, string>();
        if (peripheral is PeripheralCategory.Joystick or PeripheralCategory.CdiGamepad)
        {
            commands[EmulationControllerVisualControl.DirectionUp] = "Up";
            commands[EmulationControllerVisualControl.DirectionDown] = "Down";
            commands[EmulationControllerVisualControl.DirectionLeft] = "Left";
            commands[EmulationControllerVisualControl.DirectionRight] = "Right";
            commands[EmulationControllerVisualControl.PrimaryAction] = peripheral == PeripheralCategory.Joystick ? "Fire" : "Action1";
        }
        if (peripheral == PeripheralCategory.CdiGamepad)
        {
            commands[EmulationControllerVisualControl.SecondaryAction] = "Action2";
            commands[EmulationControllerVisualControl.TertiaryAction] = "Action3";
        }
        if (peripheral == PeripheralCategory.Mouse)
        {
            commands[EmulationControllerVisualControl.MouseLeftButton] = "Action1";
            commands[EmulationControllerVisualControl.MouseRightButton] = "Action2";
        }
        return commands;
    }
}
