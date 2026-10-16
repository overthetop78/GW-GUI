using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateSonyNeGcon() => new(ControllerArtworkFileNames.SonyNeGcon,
    [
        new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 12.0d, 25.0d, 19.0d, 35.0d),
        new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 12.0d, 25.0d, 19.0d, 35.0d),
        new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 12.0d, 25.0d, 19.0d, 35.0d),
        new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 12.0d, 25.0d, 19.0d, 35.0d),
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 28.9d, 57.6d, 6.0d, 12.5d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 82.0d, 39.0d, 9.0d, 17.0d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 74.0d, 23.0d, 9.0d, 16.0d),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 75.0d, 53.0d, 6.0d, 13.0d),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 67.4d, 36.7d, 6.0d, 13.0d),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 14.0d, 3.0d, 21.0d, 7.0d),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 66.0d, 3.0d, 21.0d, 7.0d),
        new(EmulationControllerVisualControl.Steering, ControllerVisualZoneShape.RoundedRectangle, 45.0d, 10.0d, 12.0d, 65.0d)
    ]);
}
