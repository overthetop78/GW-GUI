using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyBeatmaniaIidx() => new(ControllerArtworkFileNames.SonyBeatmaniaIidx,
    [
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 57.2, 44.2, 6, 21.6),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 61.7, 15.9, 5.7, 20.2),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 67, 44.2, 6, 21.6),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 70.7, 15.9, 5.7, 20.2),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 76.5, 44.2, 6, 21.6),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.RoundedRectangle, 80.1, 15.9, 5.7, 20.2),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.RoundedRectangle, 86.2, 44.2, 6, 21.6),
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.Ellipse, 7.1, 7.7, 35, 71.6),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.Ellipse, 7.1, 7.7, 35, 71.6),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 54.1, 16.1, 3.3, 7.7),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 54.1, 27, 3.3, 7.7)
    ]);
}
