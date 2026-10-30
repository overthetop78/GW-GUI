using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyBiohazardPad() => new(ControllerArtworkFileNames.SonyBiohazardPad,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 11.9, 16, 16.8, 22.7),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 11.9, 16, 16.8, 22.7),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 11.9, 16, 16.8, 22.7),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 11.9, 16, 16.8, 22.7),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.RoundedRectangle, 65.2, 23.8, 8.2, 12),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.RoundedRectangle, 64.1, 15, 5.3, 7.2),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.RoundedRectangle, 73, 7.8, 8.2, 9.6),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 38.9, 20.1, 9.2, 9.1),
        new(EmulationControllerVisualControl.LeftTrigger, ControllerVisualZoneShape.RoundedRectangle, 39.1, 10.7, 9.4, 8.8),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 48.6, 20.1, 8.8, 9.1),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 76.2, 18.7, 8.8, 13.9),
        new(EmulationControllerVisualControl.RightTrigger, ControllerVisualZoneShape.RoundedRectangle, 48.6, 10.7, 8.8, 8.8),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 39.8, 32.4, 5.5, 4.8),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 52.1, 32.4, 5.9, 4.8)
    ]);
}
