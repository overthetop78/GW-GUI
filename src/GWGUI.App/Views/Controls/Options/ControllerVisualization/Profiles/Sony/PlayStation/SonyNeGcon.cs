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
        new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.Ellipse, 27.0d, 63.0d, 9.0d, 15.0d),
        new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 82.0d, 39.0d, 9.0d, 17.0d),
        new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 74.0d, 23.0d, 9.0d, 16.0d),
        new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 69.0d, 59.0d, 10.0d, 18.0d),
        new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 62.0d, 44.0d, 10.0d, 18.0d),
        new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 14.0d, 3.0d, 21.0d, 7.0d),
        new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 66.0d, 3.0d, 21.0d, 7.0d),
        new(EmulationControllerVisualControl.Steering, ControllerVisualZoneShape.RoundedRectangle, 45.0d, 10.0d, 12.0d, 65.0d)
    ]);
}
