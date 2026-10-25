using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyAsciiSpecializedPad() => new(ControllerArtworkFileNames.SonyAsciiSpecializedPad,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 14.6, 30, 18.5, 22.5),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 14.6, 30, 18.5, 22.5),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 14.6, 30, 18.5, 22.5),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 14.6, 30, 18.5, 22.5),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 72, 46, 10, 11.5),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 80, 35.7, 10, 11.5),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 63.5, 35.7, 10, 11.5),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 72, 25.8, 10, 11.5),
        new(EmulationControllerVisualControl.Option, ControllerVisualZoneShape.RoundedRectangle, 41.5, 51.7, 4.5, 6.2),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 49, 51.7, 6, 6.2),
        new(EmulationControllerVisualControl.Slow, ControllerVisualZoneShape.RoundedRectangle, 55.8, 52, 3, 5.7),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 20, 8, 10, 4),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 70, 8, 10, 4)
    ]);
}
