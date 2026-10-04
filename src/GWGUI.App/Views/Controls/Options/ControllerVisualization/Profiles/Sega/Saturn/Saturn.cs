using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSaturn() => new(ControllerArtworkFileNames.Saturn,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 15d, 39d, 19d, 28d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 15d, 39d, 19d, 28d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 15d, 39d, 19d, 28d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 15d, 39d, 19d, 28d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 62d, 58d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 71d, 51d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 80d, 46d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 59d, 43d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 67d, 36d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceZ, ControllerVisualZoneShape.Ellipse, 76d, 33d, 9d, 13d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 45d, 56d, 10d, 8d),
                new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 15d, 12d, 16d, 12d),
                new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 68d, 12d, 17d, 12d)
            ]);
}
