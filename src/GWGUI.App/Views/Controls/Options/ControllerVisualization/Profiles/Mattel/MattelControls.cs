using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateMattelIntellivisionController() => new(ControllerArtworkFileNames.MattelIntellivisionController,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 44, 58, 12, 8),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 44, 72, 12, 8),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 32, 65, 12, 8),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 56, 65, 12, 8),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 19, 27, 7, 6),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 74, 27, 7, 6),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 19, 37, 7, 6),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 74, 37, 7, 6),
        new(EmulationControllerVisualControl.Key1, ControllerVisualZoneShape.Ellipse, 34, 23, 9, 6),
        new(EmulationControllerVisualControl.Key2, ControllerVisualZoneShape.Ellipse, 45.5, 23, 9, 6),
        new(EmulationControllerVisualControl.Key3, ControllerVisualZoneShape.Ellipse, 57, 23, 9, 6),
        new(EmulationControllerVisualControl.Key4, ControllerVisualZoneShape.Ellipse, 34, 30, 9, 6),
        new(EmulationControllerVisualControl.Key5, ControllerVisualZoneShape.Ellipse, 45.5, 30, 9, 6),
        new(EmulationControllerVisualControl.Key6, ControllerVisualZoneShape.Ellipse, 57, 30, 9, 6),
        new(EmulationControllerVisualControl.Key7, ControllerVisualZoneShape.Ellipse, 34, 37.5, 9, 6),
        new(EmulationControllerVisualControl.Key8, ControllerVisualZoneShape.Ellipse, 45.5, 37.5, 9, 6),
        new(EmulationControllerVisualControl.Key9, ControllerVisualZoneShape.Ellipse, 57, 37.5, 9, 6),
        new(EmulationControllerVisualControl.KeyStar, ControllerVisualZoneShape.Ellipse, 34, 45, 9, 6),
        new(EmulationControllerVisualControl.Key0, ControllerVisualZoneShape.Ellipse, 45.5, 45, 9, 6),
        new(EmulationControllerVisualControl.KeyHash, ControllerVisualZoneShape.Ellipse, 57, 45, 9, 6)
    ]);
}
