using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyAsciiEnhancedPad() => new(ControllerArtworkFileNames.SonyAsciiEnhancedPad,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 13.5, 24.1, 15.6, 24.1),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 13.5, 24.1, 15.6, 24.1),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 13.5, 24.1, 15.6, 24.1),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 13.5, 24.1, 15.6, 24.1),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 74.4, 41.8, 8.2, 12.7),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 82.8, 29.5, 8.2, 12.7),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 66.2, 29.5, 8.2, 12.7),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 74.2, 16.9, 8.2, 12.7),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 16, 0.5, 13, 6),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 71, 0.5, 13, 6),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 47.4, 49.1, 5.3, 6),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 56.6, 49.1, 6.3, 6),
        new(EmulationControllerVisualControl.Slow, ControllerVisualZoneShape.RoundedRectangle, 39.5, 48.2, 2.2, 7.2)
    ]);
}
