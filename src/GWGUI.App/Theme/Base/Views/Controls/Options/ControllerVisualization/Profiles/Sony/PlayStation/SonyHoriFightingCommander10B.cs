using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyHoriFightingCommander10B() => new(ControllerArtworkFileNames.SonyHoriFightingCommander10B,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 8.4, 26.3, 19.5, 30.6),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 8.4, 26.3, 19.5, 30.6),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 8.4, 26.3, 19.5, 30.6),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 8.4, 26.3, 19.5, 30.6),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 68.2, 49.8, 8.6, 13.5),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 77.2, 38.2, 8.6, 13.5),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 63.7, 33.9, 8.6, 13.5),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 72.8, 23.2, 8.6, 13.5),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 83.8, 17.7, 8.6, 13.5),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 88.9, 32.4, 8.6, 13.5),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 12, 0, 15, 5),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 76, 0, 15, 5),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 42.8, 57.2, 5.3, 5.8),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 52.5, 56.9, 6.6, 5.5),
        new(EmulationControllerVisualControl.Slow, ControllerVisualZoneShape.RoundedRectangle, 36.1, 52.9, 2.2, 8.3)
    ]);
}
