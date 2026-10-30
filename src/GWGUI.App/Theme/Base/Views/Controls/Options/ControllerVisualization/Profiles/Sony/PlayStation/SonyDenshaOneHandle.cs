using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDenshaOneHandle() => new(ControllerArtworkFileNames.SonyDenshaOneHandle,
    [
        new(EmulationControllerVisualControl.CombinedLeverForward, ControllerVisualZoneShape.RoundedRectangle, 15.62, 1.53, 64.84, 4.60),
        new(EmulationControllerVisualControl.CombinedLeverBackward, ControllerVisualZoneShape.RoundedRectangle, 15.62, 6.14, 64.84, 4.60),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 71.09, 41.43, 5.47, 4.09),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 82.42, 41.43, 5.66, 4.09),
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 70.51, 55.24, 5.47, 7.16),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 77.93, 63.43, 5.47, 7.16),
        new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 84.57, 55.24, 5.47, 7.16)
    ]);
}
