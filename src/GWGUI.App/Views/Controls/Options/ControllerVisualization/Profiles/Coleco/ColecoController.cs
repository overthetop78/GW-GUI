using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateColecoController() => new(ControllerArtworkFileNames.ColecoController,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 33, 6, 9, 8),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 35, 18, 9, 8),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 26, 11, 9, 8),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 43, 11, 9, 8),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 23, 37, 13, 10),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 56, 22, 10, 10),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.Ellipse, 42, 50, 10, 8),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.Ellipse, 52, 45, 10, 8),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.Ellipse, 61, 40, 10, 8),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.Ellipse, 48, 59, 10, 8),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.Ellipse, 58, 54, 10, 8),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.Ellipse, 67, 49, 10, 8),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.Ellipse, 54, 69, 10, 8),
        new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.Ellipse, 63, 64, 10, 8),
        new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.Ellipse, 72, 59, 10, 8),
        new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.Ellipse, 60, 78, 10, 8),
        new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.Ellipse, 69, 73, 10, 8),
        new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.Ellipse, 78, 68, 10, 8)
    ]);
}
