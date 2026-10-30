using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyAsciiArcadeStick() => new(ControllerArtworkFileNames.SonyAsciiArcadeStick,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 18, 35, 14, 21),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 18, 35, 14, 21),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 18, 35, 14, 21),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 18, 35, 14, 21),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 53.2, 40, 10.8, 15),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 63, 37.5, 10.8, 15),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 50, 25.8, 10.8, 15),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 59.5, 24.5, 10.8, 15),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 40.5, 33.1, 10.8, 15),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 44, 46.5, 10.8, 15),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 69.3, 24.5, 10.8, 15),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 73.1, 37.5, 10.8, 15),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 82.4, 12.6, 3, 4),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 86.8, 12.6, 4, 4),
        new(EmulationControllerVisualControl.Slow, ControllerVisualZoneShape.RoundedRectangle, 76.2, 12.6, 3.5, 4)
    ]);
}
