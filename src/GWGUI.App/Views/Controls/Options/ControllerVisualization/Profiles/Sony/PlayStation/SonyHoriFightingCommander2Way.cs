using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyHoriFightingCommander2Way() => new(ControllerArtworkFileNames.SonyHoriFightingCommander2Way,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 10.7, 32.5, 17.6, 39),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 10.7, 32.5, 17.6, 39),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 10.7, 32.5, 17.6, 39),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 10.7, 32.5, 17.6, 39),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 67.4, 61.9, 8.2, 18.2),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 76, 46.3, 8.2, 18.2),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 63.5, 40.7, 8.2, 18.2),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 72.1, 26, 8.2, 18.2),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 82, 21.6, 7.8, 17.3),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 86.3, 40.3, 7.8, 17.3),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 14, 0.7, 16, 6),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 74, 0.7, 16, 6),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 42.2, 64.9, 5.3, 7.4),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 50.8, 64.9, 7, 7.4)
    ]);
}
