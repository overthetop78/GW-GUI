using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreatePhilipsVideopacJoystick() => new(
        ControllerArtworkFileNames.PhilipsVideopacJoystick,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 43, 34, 17, 9),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 43, 50, 17, 9),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 33, 41, 13, 12),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 56, 41, 13, 12),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 17, 11, 23, 16)
    ]);

    private static ProfileDefinition CreatePhilipsCdiGamepad() => new(
        ControllerArtworkFileNames.PhilipsCdiGamepad,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.Ellipse, 22, 27, 9, 9),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.Ellipse, 22, 43, 9, 10),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.Ellipse, 16, 35, 9, 10),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.Ellipse, 30, 35, 9, 10),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 59, 42, 10, 14),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 68, 34, 10, 14),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 77, 25, 10, 14)
    ]);

    private static ProfileDefinition CreatePhilipsCdiMouse() => new(
        ControllerArtworkFileNames.PhilipsCdiMouse,
    [
        new(EmulationControllerVisualControl.MouseLeftButton, ControllerVisualZoneShape.RoundedRectangle, 14, 5, 34, 28),
        new(EmulationControllerVisualControl.MouseRightButton, ControllerVisualZoneShape.RoundedRectangle, 52, 5, 34, 28)
    ]);
}
