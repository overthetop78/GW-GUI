using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyAsciiFighterStickV() => new(ControllerArtworkFileNames.SonyAsciiFighterStickV,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 14.7, 38.5, 14.9, 18.8),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 14.7, 38.5, 14.9, 18.8),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 14.7, 38.5, 14.9, 18.8),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 14.7, 38.5, 14.9, 18.8),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 52.7, 37.5, 9.4, 11.8),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 63.9, 34.8, 9.4, 11.8),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 49.2, 24.2, 9.4, 11.8),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 60.4, 21.5, 9.4, 11.8),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 39.3, 32.1, 9.4, 11.8),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 43.2, 44.4, 9.4, 11.8),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 70.7, 21.2, 9.4, 11.8),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 74.6, 33.6, 9.4, 11.8),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 85.9, 9.6, 3.1, 4),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 91.6, 9.6, 3.5, 4),
        new(EmulationControllerVisualControl.Slow, ControllerVisualZoneShape.Ellipse, 79.1, 10.1, 3.1, 3)
    ]);
}
