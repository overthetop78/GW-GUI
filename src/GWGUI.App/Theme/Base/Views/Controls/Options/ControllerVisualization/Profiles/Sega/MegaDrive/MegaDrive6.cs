using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateMegaDrive6() => new(ControllerArtworkFileNames.MegaDrive6,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 14d, 38d, 18d, 30d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 14d, 38d, 18d, 30d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 14d, 38d, 18d, 30d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 14d, 38d, 18d, 30d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 64d, 54d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 75d, 52d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 84d, 49d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 64d, 41d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 71d, 37d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceZ, ControllerVisualZoneShape.Ellipse, 79d, 36d, 9d, 13d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 46d, 45d, 9d, 7d),
                new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 46d, 57d, 9d, 7d)
            ]);
}
