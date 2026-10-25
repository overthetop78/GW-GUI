using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyNamcoArcadeStick() => new(ControllerArtworkFileNames.SonyNamcoArcadeStick,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 21, 21, 14, 21),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 21, 21, 14, 21),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 21, 21, 14, 21),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 21, 21, 14, 21),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 47, 43, 12.3, 19.5),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 59.2, 36.5, 12.3, 19.5),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 50.5, 22, 12.3, 19.5),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 62, 15.8, 12.3, 19.5),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 75, 6.2, 4.4, 6.8),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 82.3, 6.2, 4.4, 6.8),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 74.8, 15.8, 12.3, 19.5),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 72.7, 36.5, 12.3, 19.5),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 53.3, 6.2, 4.2, 6.8),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 63.2, 6.2, 4.2, 6.8)
    ]);
}
