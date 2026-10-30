using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyHoriFightingStick2() => new(ControllerArtworkFileNames.SonyHoriFightingStick2,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.JoystickDirection, 16.37, 20.88, 13.36, 19.98),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.JoystickDirection, 16.37, 20.88, 13.36, 19.98),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.JoystickDirection, 16.37, 20.88, 13.36, 19.98),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.JoystickDirection, 16.37, 20.88, 13.36, 19.98),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 45.49, 33.67, 8.28, 12.39),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 55.78, 24.98, 8.28, 12.39),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 67.07, 22.18, 8.28, 12.39),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 78.82, 22.58, 8.28, 12.39),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 45.62, 49.65, 8.28, 12.39),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 56.18, 42.36, 8.28, 12.39),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 67.74, 39.26, 8.28, 12.39),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 79.63, 39.16, 8.28, 12.39),
        new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 40.88, 69.93, 6.01, 8.99),
        new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 52.71, 69.93, 6.01, 8.99),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 62.86, 9.79, 3.21, 4.8),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 68.6, 9.79, 3.21, 4.8),
        new(EmulationControllerVisualControl.AnalogMode, ControllerVisualZoneShape.Ellipse, 77.22, 9.79, 3.21, 4.8),
        new(EmulationControllerVisualControl.Turbo, ControllerVisualZoneShape.Ellipse, 20.11, 10.39, 3.21, 4.8),
        new(EmulationControllerVisualControl.RepeatRate, ControllerVisualZoneShape.RoundedRectangle, 14.09, 10.29, 1.2, 4),
        new(EmulationControllerVisualControl.StickMode, ControllerVisualZoneShape.RoundedRectangle, 84.77, 10.29, 1.2, 4)
    ]);
}
