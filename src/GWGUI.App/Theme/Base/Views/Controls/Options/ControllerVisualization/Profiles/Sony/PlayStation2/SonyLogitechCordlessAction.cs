using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyLogitechCordlessAction() => new(ControllerArtworkFileNames.SonyLogitechCordlessAction,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 7.03, 27.35, 18.75, 28.24),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 7.03, 27.35, 18.75, 28.24),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 7.03, 27.35, 18.75, 28.24),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 7.03, 27.35, 18.75, 28.24),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.JoystickDirection, 25, 56.18, 15.23, 22.94),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.JoystickDirection, 25, 56.18, 15.23, 22.94),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.JoystickDirection, 25, 56.18, 15.23, 22.94),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.JoystickDirection, 25, 56.18, 15.23, 22.94),
        new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 29.95, 63.63, 5.33, 8.03),
        new(EmulationControllerVisualControl.RightStickUp, ControllerVisualZoneShape.JoystickDirection, 57.23, 56.47, 15.23, 22.94),
        new(EmulationControllerVisualControl.RightStickDown, ControllerVisualZoneShape.JoystickDirection, 57.23, 56.47, 15.23, 22.94),
        new(EmulationControllerVisualControl.RightStickLeft, ControllerVisualZoneShape.JoystickDirection, 57.23, 56.47, 15.23, 22.94),
        new(EmulationControllerVisualControl.RightStickRight, ControllerVisualZoneShape.JoystickDirection, 57.23, 56.47, 15.23, 22.94),
        new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 62.18, 63.93, 5.33, 8.03),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 72.46, 36.76, 6.64, 10),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 81.84, 23.24, 6.64, 10),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 80.86, 50.29, 6.64, 10),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 89.65, 36.47, 6.64, 10),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 37.4, 40.44, 5.27, 3.82),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 57.32, 40.44, 5.27, 3.82),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.Ellipse, 31.84, 21.47, 4.69, 7.06),
        new(EmulationControllerVisualControl.VibrationMode, ControllerVisualZoneShape.Ellipse, 63.67, 21.47, 4.69, 7.06)
    ]);
}
