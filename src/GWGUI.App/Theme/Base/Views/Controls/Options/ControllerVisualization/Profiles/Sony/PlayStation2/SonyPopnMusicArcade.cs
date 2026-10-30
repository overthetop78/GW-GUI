using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyPopnMusicArcade() => new(ControllerArtworkFileNames.SonyPopnMusicArcade,
    [
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.Ellipse, 5, 57.5, 11.7, 24),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.Ellipse, 16.5, 10, 10.7, 25.5),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.Ellipse, 24.8, 57.5, 11.7, 24),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.Ellipse, 35.6, 10, 10.7, 25.5),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.Ellipse, 44.2, 57.5, 11.7, 24),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.Ellipse, 54.8, 10, 10.7, 25.5),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.Ellipse, 63.9, 57.5, 11.7, 24),
        new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.Ellipse, 74.2, 10, 10.7, 25.5),
        new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.Ellipse, 83.4, 57.5, 11.7, 24),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 47.1, 3.8, 2.1, 5),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 49.9, 3.8, 2.1, 5)
    ]);
}
