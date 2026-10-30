using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyMultiTrain() => new(ControllerArtworkFileNames.SonyMultiTrain,
    [
        new(EmulationControllerVisualControl.CombinedLeverForward, ControllerVisualZoneShape.RoundedRectangle, 17.19, 1.11, 66.80, 4.88),
        new(EmulationControllerVisualControl.CombinedLeverBackward, ControllerVisualZoneShape.RoundedRectangle, 17.19, 6.21, 66.80, 4.88),
        new(EmulationControllerVisualControl.ReverserForward, ControllerVisualZoneShape.RoundedRectangle, 90.82, 23.28, 5.66, 6.21),
        new(EmulationControllerVisualControl.ReverserNeutral, ControllerVisualZoneShape.RoundedRectangle, 90.82, 31.04, 5.66, 5.54),
        new(EmulationControllerVisualControl.ReverserBackward, ControllerVisualZoneShape.RoundedRectangle, 90.82, 38.80, 5.66, 5.54),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 78.91, 62.75, 4.30, 2.66),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 86.91, 62.75, 4.30, 2.66),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 67.19, 58.20, 2.73, 3.77),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 67.19, 65.74, 2.73, 3.77),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 63.28, 62.20, 2.73, 3.77),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 71.29, 62.20, 2.73, 3.77),
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 62.30, 50.33, 5.47, 6.21),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 70.12, 50.33, 5.47, 6.21),
        new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 77.93, 50.33, 5.47, 6.21),
        new(EmulationControllerVisualControl.FaceD, ControllerVisualZoneShape.Ellipse, 85.74, 50.33, 5.47, 6.21),
        new(EmulationControllerVisualControl.Ats, ControllerVisualZoneShape.Ellipse, 83.30, 85.25, 6.84, 7.76)
    ]);
}
