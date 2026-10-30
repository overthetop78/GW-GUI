using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyGameTrak() => new(ControllerArtworkFileNames.SonyGameTrak,
    [
        new(EmulationControllerVisualControl.LeftTetherLeft, ControllerVisualZoneShape.RoundedRectangle, 30.859, 20.062, 1.953, 4.321),
        new(EmulationControllerVisualControl.LeftTetherRight, ControllerVisualZoneShape.RoundedRectangle, 36.523, 20.062, 1.953, 4.321),
        new(EmulationControllerVisualControl.LeftTetherForward, ControllerVisualZoneShape.RoundedRectangle, 33.008, 15.741, 2.734, 3.086),
        new(EmulationControllerVisualControl.LeftTetherBackward, ControllerVisualZoneShape.RoundedRectangle, 33.008, 25.309, 2.734, 3.086),
        new(EmulationControllerVisualControl.LeftTetherExtend, ControllerVisualZoneShape.RoundedRectangle, 37.305, 6.173, 2.148, 4.321),
        new(EmulationControllerVisualControl.LeftTetherRetract, ControllerVisualZoneShape.RoundedRectangle, 35.352, 12.037, 2.148, 3.704),
        new(EmulationControllerVisualControl.RightTetherLeft, ControllerVisualZoneShape.RoundedRectangle, 61.328, 20.062, 1.953, 4.321),
        new(EmulationControllerVisualControl.RightTetherRight, ControllerVisualZoneShape.RoundedRectangle, 66.992, 20.062, 1.953, 4.321),
        new(EmulationControllerVisualControl.RightTetherForward, ControllerVisualZoneShape.RoundedRectangle, 63.477, 15.741, 2.734, 3.086),
        new(EmulationControllerVisualControl.RightTetherBackward, ControllerVisualZoneShape.RoundedRectangle, 63.477, 25.309, 2.734, 3.086),
        new(EmulationControllerVisualControl.RightTetherExtend, ControllerVisualZoneShape.RoundedRectangle, 67.773, 6.173, 2.148, 4.321),
        new(EmulationControllerVisualControl.RightTetherRetract, ControllerVisualZoneShape.RoundedRectangle, 65.820, 12.037, 2.148, 3.704),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 37.305, 75.926, 25.391, 18.519)
    ]);
}
