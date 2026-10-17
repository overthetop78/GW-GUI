using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDrivingForce() => new(ControllerArtworkFileNames.SonyDrivingForce,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 16.67, 11.72, 9.77, 6.64),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 16.67, 11.72, 9.77, 6.64),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 16.67, 11.72, 9.77, 6.64),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 16.67, 11.72, 9.77, 6.64),
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.Ellipse, 3.16, 32.23, 8.05, 5.47),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.Ellipse, 89.08, 32.42, 8.05, 5.47),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 78.16, 12.5, 2.87, 1.95),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 73.85, 15.04, 2.87, 1.95),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 82.76, 15.04, 2.87, 1.95),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 78.16, 17.58, 2.87, 1.95),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 26.44, 26.76, 4.02, 2.34),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 22.99, 33.01, 3.45, 2.34),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 69.54, 26.95, 4.02, 2.34),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 72.99, 33.01, 3.45, 2.34),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 44.25, 46.78, 4.02, 1.37),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 50.57, 46.78, 4.02, 1.37),
        new(EmulationControllerVisualControl.LeftStick, ControllerVisualZoneShape.RoundedRectangle, 22.13, 23.05, 4.02, 2.93),
        new(EmulationControllerVisualControl.RightStick, ControllerVisualZoneShape.RoundedRectangle, 72.13, 22.85, 4.02, 2.93),
        new(EmulationControllerVisualControl.BrakePedal, ControllerVisualZoneShape.RoundedRectangle, 26.15, 64.65, 18.97, 14.65),
        new(EmulationControllerVisualControl.Accelerator, ControllerVisualZoneShape.RoundedRectangle, 58.05, 62.3, 15.52, 17.77)
    ]);
}
