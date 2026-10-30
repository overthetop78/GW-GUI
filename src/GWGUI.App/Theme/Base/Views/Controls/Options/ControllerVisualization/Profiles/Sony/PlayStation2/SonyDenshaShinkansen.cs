using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDenshaShinkansen() => new(ControllerArtworkFileNames.SonyDenshaShinkansen,
    [
        new(EmulationControllerVisualControl.PowerDecrease, ControllerVisualZoneShape.RoundedRectangle, 1.73, 21.09, 21.21, 8.79),
        new(EmulationControllerVisualControl.PowerIncrease, ControllerVisualZoneShape.RoundedRectangle, 1.73, 29.88, 21.21, 8.79),
        new(EmulationControllerVisualControl.BrakeDecrease, ControllerVisualZoneShape.RoundedRectangle, 77.06, 21.09, 21.21, 8.79),
        new(EmulationControllerVisualControl.BrakeIncrease, ControllerVisualZoneShape.RoundedRectangle, 77.06, 29.88, 21.21, 8.79),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 43.51, 38.67, 4.33, 1.95),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 52.16, 38.67, 4.33, 1.95),
        new(EmulationControllerVisualControl.HornPedal, ControllerVisualZoneShape.RoundedRectangle, 43.72, 71.09, 12.55, 11.13),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 35.93, 41.99, 3.46, 2.34),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 35.93, 47.66, 3.46, 2.34),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 31.39, 45.12, 3.46, 2.34),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 40.04, 45.12, 3.46, 2.34),
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 55.74, 44.43, 3.68, 3.32),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 60.28, 47.56, 3.68, 3.32),
        new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 64.39, 44.43, 3.68, 3.32),
        new(EmulationControllerVisualControl.FaceD, ControllerVisualZoneShape.Ellipse, 60.28, 41.50, 3.68, 3.32)
    ]);
}
