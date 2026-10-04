using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateMegaDrive3() => new(ControllerArtworkFileNames.MegaDrive3,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 15d, 37d, 18d, 28d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 15d, 37d, 18d, 28d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 15d, 37d, 18d, 28d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 15d, 37d, 18d, 28d),
                new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 62d, 52d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 71d, 45d, 9d, 13d),
                new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 80d, 39d, 9d, 13d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 46d, 47d, 9d, 7d)
            ]);
}
