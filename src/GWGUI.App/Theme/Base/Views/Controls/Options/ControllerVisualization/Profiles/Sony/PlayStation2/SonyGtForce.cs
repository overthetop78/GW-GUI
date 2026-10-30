using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyGtForce() => new(ControllerArtworkFileNames.SonyGtForce,
    [
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.Ellipse, 5.41, 29.49, 8.78, 5.08),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.Ellipse, 85.47, 29.49, 8.78, 5.08),
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 26.35, 23.83, 2.7, 1.56),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 25.34, 30.47, 2.7, 1.56),
        new(EmulationControllerVisualControl.FaceX, ControllerVisualZoneShape.Ellipse, 70.95, 23.83, 2.7, 1.56),
        new(EmulationControllerVisualControl.FaceY, ControllerVisualZoneShape.Ellipse, 72.64, 30.47, 2.7, 1.56),
        new(EmulationControllerVisualControl.BrakePedal, ControllerVisualZoneShape.RoundedRectangle, 20.27, 59.57, 24.32, 14.65),
        new(EmulationControllerVisualControl.Accelerator, ControllerVisualZoneShape.RoundedRectangle, 65.2, 57.03, 17.23, 21.68)
    ]);
}
