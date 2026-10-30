using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDrivingForcePro() => new(ControllerArtworkFileNames.SonyDrivingForcePro,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 18.42, 23.83, 7.89, 5.86),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 18.42, 23.83, 7.89, 5.86),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 18.42, 23.83, 7.89, 5.86),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 18.42, 23.83, 7.89, 5.86),
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.Ellipse, 4.47, 27.73, 7.89, 5.86),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.Ellipse, 67.89, 27.73, 7.89, 5.86),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 56.84, 23.05, 2.11, 1.56),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 53.95, 25.39, 2.11, 1.56),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 60, 25.39, 2.11, 1.56),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 57.11, 27.93, 2.11, 1.56),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 27.37, 22.07, 3.16, 1.17),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 50.26, 22.07, 3.16, 1.17),
        new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.RoundedRectangle, 36.58, 37.89, 3.16, 1.17),
        new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.RoundedRectangle, 41.58, 37.89, 3.16, 1.17),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 36.84, 41.89, 2.63, 0.98),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 42.11, 41.89, 2.63, 0.98),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 13.95, 20.61, 2.63, 1.76),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 63.68, 20.61, 2.63, 1.76),
        new(EmulationControllerVisualControl.GearUp, ControllerVisualZoneShape.RoundedRectangle, 84.74, 19.34, 11.05, 3.91),
        new(EmulationControllerVisualControl.GearDown, ControllerVisualZoneShape.RoundedRectangle, 84.74, 23.24, 11.05, 3.91),
        new(EmulationControllerVisualControl.BrakePedal, ControllerVisualZoneShape.RoundedRectangle, 20.53, 59.57, 14.21, 15.23),
        new(EmulationControllerVisualControl.Accelerator, ControllerVisualZoneShape.RoundedRectangle, 49.47, 58.4, 13.95, 15.82)
    ]);
}
