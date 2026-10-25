using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyBeatmaniaIidxArcade() => new(ControllerArtworkFileNames.SonyBeatmaniaIidxArcade,
    [
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 62, 62.7, 3.6, 9.9),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 65.1, 49.8, 3.5, 9.6),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 68.6, 62.7, 3.6, 9.9),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 71.7, 49.8, 3.5, 9.6),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 75.1, 62.7, 3.6, 9.9),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.RoundedRectangle, 78.1, 49.8, 3.5, 9.6),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.RoundedRectangle, 81.9, 62.7, 3.6, 9.9),
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.Ellipse, 12.7, 24.1, 35.9, 55.3),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.Ellipse, 12.7, 24.1, 35.9, 55.3),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 65.6, 22.6, 3.1, 9.1),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 78.4, 22.6, 3.1, 9.1)
    ]);
}
