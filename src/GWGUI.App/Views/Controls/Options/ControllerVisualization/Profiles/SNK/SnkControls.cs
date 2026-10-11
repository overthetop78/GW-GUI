using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSnkNeoGeoArcadeStick() => new(ControllerArtworkFileNames.SnkNeoGeoArcadeStick,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 27, 19, 7, 8),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 27, 36, 7, 8),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 20, 27, 7, 8),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 35, 27, 7, 8),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 51, 35, 8, 8),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 51, 26, 8, 8),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 54, 18, 8, 8),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 58, 12, 8, 8),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 43, 13, 6, 5),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 34, 18, 6, 5)
    ]);

    private static ProfileDefinition CreateSnkNeoGeoCdPad() => new(ControllerArtworkFileNames.SnkNeoGeoCdPad,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 23, 17, 7, 9),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 23, 37, 7, 9),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 13, 27, 7, 9),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 33, 27, 7, 9),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 63, 54, 10, 13),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 76, 51, 10, 13),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 62, 36, 10, 13),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 75, 32, 10, 13),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 42, 55, 10, 7),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 47, 40, 10, 7)
    ]);

    private static ProfileDefinition CreateSnkNeoGeoPocket() => new(ControllerArtworkFileNames.SnkNeoGeoPocket,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 13, 25, 6, 8),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 13, 44, 6, 8),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 7, 35, 6, 8),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 20, 35, 6, 8),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 77, 38, 8, 12),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 86, 25, 8, 12),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 89, 9, 5, 7)
    ]);

    private static ProfileDefinition CreateSnkNeoGeoPocketColor() => new(ControllerArtworkFileNames.SnkNeoGeoPocketColor,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 12, 31, 6, 8),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 12, 50, 6, 8),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 6, 40, 6, 8),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 19, 40, 6, 8),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 76, 37, 8, 12),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 85, 26, 8, 12),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 87, 10, 5, 7)
    ]);
}
