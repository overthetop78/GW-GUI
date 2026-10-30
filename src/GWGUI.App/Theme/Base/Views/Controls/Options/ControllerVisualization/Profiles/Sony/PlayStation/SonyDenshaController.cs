using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyDenshaController() => new(ControllerArtworkFileNames.SonyDenshaController,
    [
        new(EmulationControllerVisualControl.PowerDecrease, ControllerVisualZoneShape.RoundedRectangle, 7.81, 1.56, 14.06, 6.25),
        new(EmulationControllerVisualControl.PowerIncrease, ControllerVisualZoneShape.RoundedRectangle, 7.81, 7.81, 14.06, 6.25),
        new(EmulationControllerVisualControl.BrakeDecrease, ControllerVisualZoneShape.Ellipse, 44.92, 25.00, 11.72, 8.75),
        new(EmulationControllerVisualControl.BrakeIncrease, ControllerVisualZoneShape.Ellipse, 65.43, 16.25, 11.72, 8.75),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 42.77, 78.75, 5.08, 5.31),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 53.12, 78.75, 5.08, 5.31),
        new(EmulationControllerVisualControl.FaceA, ControllerVisualZoneShape.Ellipse, 72.95, 78.28, 4.49, 7.19),
        new(EmulationControllerVisualControl.FaceB, ControllerVisualZoneShape.Ellipse, 78.22, 84.84, 4.49, 7.19),
        new(EmulationControllerVisualControl.FaceC, ControllerVisualZoneShape.Ellipse, 83.30, 78.28, 4.49, 7.19)
    ]);
}
