using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyBeatmania() => new(ControllerArtworkFileNames.SonyBeatmania,
    [
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 9.4, 45.5, 7.6, 16.5),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 15.5, 24.8, 7.3, 16.2),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 20.2, 45.5, 7.6, 16.5),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 26.2, 24.8, 7.3, 16.2),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 31, 45.5, 7.6, 16.5),
        new(EmulationControllerVisualControl.RotaryLeft, ControllerVisualZoneShape.Ellipse, 51, 19.2, 43, 53),
        new(EmulationControllerVisualControl.RotaryRight, ControllerVisualZoneShape.Ellipse, 51, 19.2, 43, 53),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 63.3, 5, 5.3, 5.1),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 70.7, 5, 5.3, 5.1)
    ]);
}
