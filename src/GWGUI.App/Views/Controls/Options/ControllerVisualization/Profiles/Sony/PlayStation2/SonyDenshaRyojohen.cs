using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDenshaRyojohen() => new(ControllerArtworkFileNames.SonyDenshaRyojohen,
    [
        new(EmulationControllerVisualControl.PowerDecrease, ControllerVisualZoneShape.RoundedRectangle, 1.17, 1.69, 11.33, 6.18),
        new(EmulationControllerVisualControl.PowerIncrease, ControllerVisualZoneShape.RoundedRectangle, 1.17, 7.87, 11.33, 6.18),
        new(EmulationControllerVisualControl.BrakeDecrease, ControllerVisualZoneShape.Ellipse, 85.94, 37.08, 7.03, 10.96),
        new(EmulationControllerVisualControl.BrakeIncrease, ControllerVisualZoneShape.Ellipse, 91.21, 50.00, 7.23, 10.96),
        new(EmulationControllerVisualControl.LeftDoor, ControllerVisualZoneShape.RoundedRectangle, 13.67, 44.94, 5.08, 8.71),
        new(EmulationControllerVisualControl.RightDoor, ControllerVisualZoneShape.RoundedRectangle, 25.78, 44.94, 5.08, 8.71),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 27.34, 69.38, 3.91, 3.09),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 26.56, 76.40, 3.71, 3.09),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 14.16, 69.66, 3.32, 3.37),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 13.18, 77.25, 3.32, 3.37),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 9.08, 73.60, 3.32, 3.37),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 18.07, 73.60, 3.32, 3.37),
        new(EmulationControllerVisualControl.Camera, ControllerVisualZoneShape.Ellipse, 47.85, 52.53, 5.08, 7.30),
        new(EmulationControllerVisualControl.Announcement, ControllerVisualZoneShape.Ellipse, 44.63, 62.50, 10.35, 14.89),
        new(EmulationControllerVisualControl.Horn, ControllerVisualZoneShape.Ellipse, 36.62, 76.54, 5.27, 7.58)
    ]);
}
