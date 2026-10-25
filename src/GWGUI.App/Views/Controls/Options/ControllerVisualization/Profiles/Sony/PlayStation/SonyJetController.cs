using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyJetController() => new(ControllerArtworkFileNames.SonyJetController,
    [
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.RoundedRectangle, 4.49, 18.30, 4.10, 10.28),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.RoundedRectangle, 61.72, 18.30, 4.10, 10.28),
        new(EmulationControllerVisualControl.StickUp, ControllerVisualZoneShape.RoundedRectangle, 20.70, 33.08, 28.32, 3.26),
        new(EmulationControllerVisualControl.StickDown, ControllerVisualZoneShape.RoundedRectangle, 20.70, 37.34, 28.32, 3.26),
        new(EmulationControllerVisualControl.PowerIncrease, ControllerVisualZoneShape.RoundedRectangle, 66.02, 45.11, 21.09, 5.01),
        new(EmulationControllerVisualControl.PowerDecrease, ControllerVisualZoneShape.RoundedRectangle, 66.02, 50.13, 21.09, 5.01),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 6.45, 2.01, 3.91, 5.01),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 60.16, 2.01, 3.91, 5.01),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 33.01, 9.52, 3.91, 5.01),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 28.71, 13.53, 3.91, 5.01),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 37.11, 13.53, 3.91, 5.01),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 33.01, 17.54, 3.91, 5.01),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 28.12, 27.32, 4.30, 3.01),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 36.91, 27.32, 4.30, 3.01)
    ]);
}
