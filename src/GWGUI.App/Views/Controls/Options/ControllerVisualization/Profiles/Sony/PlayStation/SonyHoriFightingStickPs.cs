using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyHoriFightingStickPs() => new(ControllerArtworkFileNames.SonyHoriFightingStickPs,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 17.5, 16, 14, 24),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 17.5, 16, 14, 24),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 17.5, 16, 14, 24),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 17.5, 16, 14, 24),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 57.6, 36, 10.2, 16.5),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 70, 30.6, 10.5, 17),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 57.6, 15.8, 10.2, 16.5),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 70, 10, 10.5, 17),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 46.7, 26, 10, 16),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.Ellipse, 46.7, 45, 10, 16),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 83, 10.5, 10.8, 17),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.Ellipse, 83, 30.6, 10.8, 17),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 37.1, 84.7, 4, 6.5),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 42.5, 84.7, 4, 6.5)
    ]);
}
