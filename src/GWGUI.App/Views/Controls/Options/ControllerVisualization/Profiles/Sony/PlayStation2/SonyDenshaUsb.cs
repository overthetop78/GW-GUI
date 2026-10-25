using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDenshaUsb() => new(ControllerArtworkFileNames.SonyDenshaUsb,
    [
        new(EmulationControllerVisualControl.PowerDecrease, ControllerVisualZoneShape.RoundedRectangle, 5.47, 0.97, 19.34, 6.80),
        new(EmulationControllerVisualControl.PowerIncrease, ControllerVisualZoneShape.RoundedRectangle, 5.47, 7.77, 19.34, 6.80),
        new(EmulationControllerVisualControl.BrakeDecrease, ControllerVisualZoneShape.RoundedRectangle, 68.55, 19.90, 13.48, 4.37),
        new(EmulationControllerVisualControl.BrakeIncrease, ControllerVisualZoneShape.RoundedRectangle, 83.98, 19.90, 13.67, 4.37),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 31.64, 62.14, 3.91, 2.91),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 39.26, 62.14, 3.91, 2.91),
        new(EmulationControllerVisualControl.HornPedal, ControllerVisualZoneShape.RoundedRectangle, 35.16, 76.46, 11.72, 11.41),
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.RoundedRectangle, 19.73, 58.01, 3.12, 2.91),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.RoundedRectangle, 19.34, 66.02, 3.12, 2.91),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.RoundedRectangle, 15.43, 62.14, 3.12, 2.91),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.RoundedRectangle, 23.24, 62.14, 3.12, 2.91),
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 56.35, 61.53, 3.32, 4.13),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 60.45, 65.90, 3.32, 4.13),
        new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 64.16, 61.53, 3.32, 4.13),
        new(EmulationControllerVisualControl.FaceD, ControllerVisualZoneShape.Ellipse, 60.25, 57.40, 3.32, 4.13)
    ]);
}
