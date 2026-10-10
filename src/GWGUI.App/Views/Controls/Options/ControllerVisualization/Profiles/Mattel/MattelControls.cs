using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateMattelIntellivisionController() => new(ControllerArtworkFileNames.MattelIntellivisionController,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 50, 62, 12, 8),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 50, 76, 12, 8),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 38, 69, 12, 8),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 62, 69, 12, 8),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 22.5, 30, 7, 6),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 77.5, 30, 7, 6),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 22.5, 40, 7, 6),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 77.5, 40, 7, 6),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.Ellipse, 38.5, 26, 9, 6),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.Ellipse, 50, 26, 9, 6),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.Ellipse, 61.5, 26, 9, 6),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.Ellipse, 38.5, 33, 9, 6),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.Ellipse, 50, 33, 9, 6),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.Ellipse, 61.5, 33, 9, 6),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.Ellipse, 38.5, 40.5, 9, 6),
        new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.Ellipse, 50, 40.5, 9, 6),
        new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.Ellipse, 61.5, 40.5, 9, 6),
        new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.Ellipse, 38.5, 48, 9, 6),
        new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.Ellipse, 50, 48, 9, 6),
        new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.Ellipse, 61.5, 48, 9, 6)
    ]);
}
