using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyLogitechCordless() => new(ControllerArtworkFileNames.SonyLogitechCordless,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 12.3, 12.5, 15.23, 25),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 12.3, 12.5, 15.23, 25),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 12.3, 12.5, 15.23, 25),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 12.3, 12.5, 15.23, 25),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.JoystickDirection, 28.12, 36.22, 13.28, 21.79),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.JoystickDirection, 28.12, 36.22, 13.28, 21.79),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.JoystickDirection, 28.12, 36.22, 13.28, 21.79),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.JoystickDirection, 28.12, 36.22, 13.28, 21.79),
        new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 32.44, 43.3, 4.65, 7.63),
        new(EmulationControllerVisualControl.RightStickUp, ControllerVisualZoneShape.JoystickDirection, 57.62, 36.22, 13.28, 21.79),
        new(EmulationControllerVisualControl.RightStickDown, ControllerVisualZoneShape.JoystickDirection, 57.62, 36.22, 13.28, 21.79),
        new(EmulationControllerVisualControl.RightStickLeft, ControllerVisualZoneShape.JoystickDirection, 57.62, 36.22, 13.28, 21.79),
        new(EmulationControllerVisualControl.RightStickRight, ControllerVisualZoneShape.JoystickDirection, 57.62, 36.22, 13.28, 21.79),
        new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 61.93, 43.3, 4.65, 7.63),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 70.31, 20.19, 5.47, 8.97),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 76.76, 9.29, 5.47, 8.97),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 77.54, 30.77, 5.47, 8.97),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 83.98, 20.19, 5.47, 8.97),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 36.43, 22.6, 4.88, 3.53),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 58.01, 22.6, 5.08, 3.53),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.Ellipse, 42.19, 15.38, 3.52, 5.77),
        new(EmulationControllerVisualControl.VibrationMode, ControllerVisualZoneShape.Ellipse, 53.32, 15.38, 3.52, 5.77)
    ]);
}
