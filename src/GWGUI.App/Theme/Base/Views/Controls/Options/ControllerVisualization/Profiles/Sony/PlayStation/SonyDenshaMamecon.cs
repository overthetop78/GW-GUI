using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDenshaMamecon() => new(ControllerArtworkFileNames.SonyDenshaMamecon,
    [
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.RoundedRectangle, 13.09, 1.56, 15.82, 7.50),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.RoundedRectangle, 71.29, 1.56, 15.82, 7.50),
        new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 76.76, 35.00, 5.47, 8.75),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 41.21, 42.50, 5.86, 5.62),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 52.15, 42.19, 5.86, 5.62),
        new(EmulationControllerVisualControl.PowerDecrease, ControllerVisualZoneShape.RoundedRectangle, 15.04, 19.06, 7.23, 17.81),
        new(EmulationControllerVisualControl.PowerIncrease, ControllerVisualZoneShape.RoundedRectangle, 15.04, 37.19, 7.23, 17.81),
        new(EmulationControllerVisualControl.BrakeDecrease, ControllerVisualZoneShape.RoundedRectangle, 65.04, 27.50, 4.88, 24.69),
        new(EmulationControllerVisualControl.BrakeIncrease, ControllerVisualZoneShape.RoundedRectangle, 88.87, 27.50, 4.88, 24.69)
    ]);
}
