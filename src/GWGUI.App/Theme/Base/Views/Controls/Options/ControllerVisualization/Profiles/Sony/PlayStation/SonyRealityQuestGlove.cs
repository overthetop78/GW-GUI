using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyRealityQuestGlove() => new(ControllerArtworkFileNames.SonyRealityQuestGlove,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 28.7, 81.7, 7.8, 15.9),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 28.7, 81.7, 7.8, 15.9),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 28.7, 81.7, 7.8, 15.9),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 28.7, 81.7, 7.8, 15.9),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 84.6, 46.6, 3.9, 8),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 82.4, 59.4, 3.9, 8),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 86.5, 32.3, 4.1, 8.4),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 88.5, 51, 3.9, 8),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.Ellipse, 90.2, 36.7, 3.5, 7.2),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.Ellipse, 86.3, 63.3, 3.5, 7.2),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.Ellipse, 81.1, 37.5, 2.7, 7.6),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 79.5, 49.8, 2.5, 8),
        new(EmulationControllerVisualControl.Reset, ControllerVisualZoneShape.Ellipse, 77.5, 61.4, 2.7, 8.8),
        new(EmulationControllerVisualControl.Guide, ControllerVisualZoneShape.Ellipse, 79.1, 70.5, 3.9, 8.8)
    ]);
}
