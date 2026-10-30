using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDrivingForceGt() => new(ControllerArtworkFileNames.SonyDrivingForceGt,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 17.03, 27.93, 7.19, 5.86),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 17.03, 27.93, 7.19, 5.86),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 17.03, 27.93, 7.19, 5.86),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 17.03, 27.93, 7.19, 5.86),
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.Ellipse, 3.6, 30.27, 7.19, 5.86),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.Ellipse, 70.74, 30.27, 7.19, 5.86),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 59.95, 27.54, 2.4, 1.95),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 56.83, 29.69, 2.4, 1.95),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 63.31, 29.69, 2.4, 1.95),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 59.95, 32.81, 2.4, 1.95),
        new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.Ellipse, 30.94, 23.83, 4.8, 3.91),
        new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.Ellipse, 45.56, 23.83, 4.8, 3.91),
        new(EmulationControllerVisualControl.Buzzer, ControllerVisualZoneShape.Ellipse, 35.25, 28.91, 10.55, 8.59),
        new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 39.09, 40.23, 2.88, 2.34),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 36.93, 46.09, 2.88, 1.17),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 41.25, 46.09, 2.88, 1.17),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 27.82, 37.7, 6, 2.93),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 27.82, 41.02, 6, 2.93),
        new(EmulationControllerVisualControl.DialLeft, ControllerVisualZoneShape.Ellipse, 45.08, 39.84, 1.92, 1.56),
        new(EmulationControllerVisualControl.DialRight, ControllerVisualZoneShape.Ellipse, 53.48, 39.84, 1.92, 1.56),
        new(EmulationControllerVisualControl.DialPress, ControllerVisualZoneShape.Ellipse, 47.48, 38.48, 5.76, 4.69),
        new(EmulationControllerVisualControl.GearUp, ControllerVisualZoneShape.RoundedRectangle, 85.13, 22.85, 10.79, 4.49),
        new(EmulationControllerVisualControl.GearDown, ControllerVisualZoneShape.RoundedRectangle, 85.13, 27.34, 10.79, 4.49),
        new(EmulationControllerVisualControl.BrakePedal, ControllerVisualZoneShape.RoundedRectangle, 25.42, 67.97, 13.67, 15.04),
        new(EmulationControllerVisualControl.Accelerator, ControllerVisualZoneShape.RoundedRectangle, 47.96, 65.23, 12.95, 16.8)
    ]);
}
