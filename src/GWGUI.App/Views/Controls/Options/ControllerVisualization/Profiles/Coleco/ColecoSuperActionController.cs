using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateColecoSuperActionController() => new(ControllerArtworkFileNames.ColecoSuperActionController,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 24, 23, 10, 6),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 24, 34, 10, 6),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 16, 28, 10, 6),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 34, 28, 10, 6),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 23, 49.5, 6, 3.7),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 29.5, 50, 6, 3.7),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 36, 50.5, 6, 3.7),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 19.5, 54, 6, 3.7),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 26.5, 54.5, 6, 3.7),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.RoundedRectangle, 33.5, 55, 6, 3.7),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.RoundedRectangle, 16.5, 58.3, 6, 3.7),
        new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.RoundedRectangle, 23, 59, 6, 3.7),
        new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.RoundedRectangle, 29.5, 59.6, 6, 3.7),
        new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.RoundedRectangle, 13.5, 63.5, 6, 3.7),
        new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.RoundedRectangle, 20.5, 64, 6, 3.7),
        new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.RoundedRectangle, 27, 64.7, 6, 3.7),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.RoundedRectangle, 9, 72.5, 7, 7),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.RoundedRectangle, 16, 73.5, 7, 7),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.RoundedRectangle, 72, 69, 6, 6),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.RoundedRectangle, 76, 73, 6, 6)
    ]);
}
