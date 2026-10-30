using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateDreamcast() => new(ControllerArtworkFileNames.Dreamcast,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 18d, 40d, 13d, 16d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 18d, 40d, 13d, 16d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 18d, 40d, 13d, 16d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 18d, 40d, 13d, 16d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 73d, 39d, 8d, 9d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 80d, 32d, 8d, 9d),
                new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 67d, 32d, 8d, 9d),
                new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 73d, 25d, 8d, 9d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 47d, 61d, 6d, 7d),
                new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 18d, 8d, 11d, 10d),
                new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 72d, 8d, 13d, 10d)
            ]);
}
