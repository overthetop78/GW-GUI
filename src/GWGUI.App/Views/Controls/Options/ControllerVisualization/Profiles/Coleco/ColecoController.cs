using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateColecoController() => new(ControllerArtworkFileNames.ColecoController,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 44, 6, 10, 6),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 44, 16, 10, 6),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 35, 11, 10, 6),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 53, 11, 10, 6),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 17, 24, 6, 12),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 76, 24, 6, 12),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.RoundedRectangle, 25.5, 46, 14, 7),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.RoundedRectangle, 42.5, 46, 14, 7),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.RoundedRectangle, 59.5, 46, 14, 7),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.RoundedRectangle, 25.5, 55, 14, 7),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.RoundedRectangle, 42.5, 55, 14, 7),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.RoundedRectangle, 59.5, 55, 14, 7),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.RoundedRectangle, 25.5, 64.5, 14, 7),
        new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.RoundedRectangle, 42.5, 64.5, 14, 7),
        new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.RoundedRectangle, 59.5, 64.5, 14, 7),
        new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.RoundedRectangle, 25.5, 73.5, 14, 7),
        new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.RoundedRectangle, 42.5, 73.5, 14, 7),
        new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.RoundedRectangle, 59.5, 73.5, 14, 7)
    ]);
}
