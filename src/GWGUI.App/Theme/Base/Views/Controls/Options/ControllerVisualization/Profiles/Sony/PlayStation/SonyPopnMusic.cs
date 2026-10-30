using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyPopnMusic() => new(ControllerArtworkFileNames.SonyPopnMusic,
    [
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.Ellipse, 7.4, 55, 13.2, 29.7),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.Ellipse, 17.8, 24.8, 13, 28.4),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.Ellipse, 25, 55, 13.6, 30.2),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.Ellipse, 35.1, 24.8, 13, 28.4),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.Ellipse, 42.9, 55, 13.8, 30.2),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.Ellipse, 52.3, 24.8, 13, 28.4),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.Ellipse, 61.2, 55, 13.6, 30.2),
        new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.Ellipse, 69.4, 24.8, 13, 28.4),
        new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.Ellipse, 79, 55, 13.6, 30.2),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 28.4, 9, 3.5, 9.1),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 33.4, 9, 3.5, 9.1)
    ]);
}
