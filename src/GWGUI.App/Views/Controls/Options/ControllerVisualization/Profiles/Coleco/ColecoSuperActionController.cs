using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateColecoSuperActionController() => new(ControllerArtworkFileNames.ColecoSuperActionController,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 34, 2, 14, 8),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 34, 17, 14, 8),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 20, 10, 14, 8),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 48, 10, 14, 8),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 27, 39, 11, 6),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 41, 39, 11, 6),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 55, 39, 11, 6),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 27, 46, 11, 6),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 41, 46, 11, 6),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.RoundedRectangle, 55, 46, 11, 6),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.RoundedRectangle, 27, 53, 11, 6),
        new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.RoundedRectangle, 41, 53, 11, 6),
        new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.RoundedRectangle, 55, 53, 11, 6),
        new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.RoundedRectangle, 27, 61, 11, 6),
        new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.RoundedRectangle, 41, 61, 11, 6),
        new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.RoundedRectangle, 55, 61, 11, 6),
        new(EmulationControllerVisualControl.StickLeft, ControllerVisualZoneShape.RoundedRectangle, 30, 74, 15, 7),
        new(EmulationControllerVisualControl.StickRight, ControllerVisualZoneShape.RoundedRectangle, 45, 74, 15, 7)
    ]);
}
