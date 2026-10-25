using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyBuzzWireless() => new(ControllerArtworkFileNames.SonyBuzzWireless,
    [
        new(EmulationControllerVisualControl.Buzz1Red, ControllerVisualZoneShape.Ellipse, 3.9, 1.1, 16.4, 13.9),
        new(EmulationControllerVisualControl.Buzz1Blue, ControllerVisualZoneShape.RoundedRectangle, 4.3, 25.8, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz1Orange, ControllerVisualZoneShape.RoundedRectangle, 4.3, 39.4, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz1Green, ControllerVisualZoneShape.RoundedRectangle, 4.3, 52.7, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz1Yellow, ControllerVisualZoneShape.RoundedRectangle, 4.3, 66.3, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz2Red, ControllerVisualZoneShape.Ellipse, 29.1, 1.1, 16.4, 13.9),
        new(EmulationControllerVisualControl.Buzz2Blue, ControllerVisualZoneShape.RoundedRectangle, 29.5, 25.8, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz2Orange, ControllerVisualZoneShape.RoundedRectangle, 29.5, 39.4, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz2Green, ControllerVisualZoneShape.RoundedRectangle, 29.5, 52.7, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz2Yellow, ControllerVisualZoneShape.RoundedRectangle, 29.5, 66.3, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz3Red, ControllerVisualZoneShape.Ellipse, 54.3, 1.1, 16.4, 13.9),
        new(EmulationControllerVisualControl.Buzz3Blue, ControllerVisualZoneShape.RoundedRectangle, 54.7, 25.8, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz3Orange, ControllerVisualZoneShape.RoundedRectangle, 54.7, 39.4, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz3Green, ControllerVisualZoneShape.RoundedRectangle, 54.7, 52.7, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz3Yellow, ControllerVisualZoneShape.RoundedRectangle, 54.7, 66.3, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz4Red, ControllerVisualZoneShape.Ellipse, 79.5, 1.1, 16.4, 13.9),
        new(EmulationControllerVisualControl.Buzz4Blue, ControllerVisualZoneShape.RoundedRectangle, 79.9, 25.8, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz4Orange, ControllerVisualZoneShape.RoundedRectangle, 79.9, 39.4, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz4Green, ControllerVisualZoneShape.RoundedRectangle, 79.9, 52.7, 15.8, 9.7),
        new(EmulationControllerVisualControl.Buzz4Yellow, ControllerVisualZoneShape.RoundedRectangle, 79.9, 66.3, 15.8, 9.7)
    ]);
}
