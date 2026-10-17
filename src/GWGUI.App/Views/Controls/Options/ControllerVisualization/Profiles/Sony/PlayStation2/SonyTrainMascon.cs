using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyTrainMascon() => new(ControllerArtworkFileNames.SonyTrainMascon,
    [
        new(EmulationControllerVisualControl.CombinedLeverForward, ControllerVisualZoneShape.RoundedRectangle, 19.34, 1.33, 61.33, 5.87),
        new(EmulationControllerVisualControl.CombinedLeverBackward, ControllerVisualZoneShape.RoundedRectangle, 19.34, 7.47, 61.33, 5.87),
        new(EmulationControllerVisualControl.ReverserForward, ControllerVisualZoneShape.RoundedRectangle, 91.41, 28.27, 5.27, 7.20),
        new(EmulationControllerVisualControl.ReverserNeutral, ControllerVisualZoneShape.RoundedRectangle, 91.41, 37.87, 5.27, 5.87),
        new(EmulationControllerVisualControl.ReverserBackward, ControllerVisualZoneShape.RoundedRectangle, 91.41, 45.33, 5.27, 5.87),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 82.62, 57.60, 3.91, 2.93),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 90.04, 57.60, 3.91, 2.93),
        new(EmulationControllerVisualControl.CloseDoors, ControllerVisualZoneShape.RoundedRectangle, 20.12, 60.53, 8.20, 9.87),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 73.24, 44.53, 3.12, 4.27),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 73.24, 54.67, 3.12, 4.27),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 69.14, 49.60, 3.12, 4.27),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 77.34, 49.60, 3.12, 4.27),
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 70.12, 65.87, 7.03, 9.60),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 79.10, 65.87, 7.03, 9.60),
        new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 88.09, 65.87, 7.03, 9.60),
        new(EmulationControllerVisualControl.Ats, ControllerVisualZoneShape.Ellipse, 12.01, 60.93, 4.88, 6.67)
    ]);
}
