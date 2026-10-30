using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateCommodoreCd32() => new(ControllerArtworkFileNames.CommodoreCd32,
            [
                new(EmulationControllerVisualControl.DirectionUp, ControllerVisualZoneShape.DirectionalPad, 5.9d, 43.1d, 13.7d, 25.5d),
                new(EmulationControllerVisualControl.DirectionDown, ControllerVisualZoneShape.DirectionalPad, 5.9d, 43.1d, 13.7d, 25.5d),
                new(EmulationControllerVisualControl.DirectionLeft, ControllerVisualZoneShape.DirectionalPad, 5.9d, 43.1d, 13.7d, 25.5d),
                new(EmulationControllerVisualControl.DirectionRight, ControllerVisualZoneShape.DirectionalPad, 5.9d, 43.1d, 13.7d, 25.5d),
                new(EmulationControllerVisualControl.PrimaryAction, ControllerVisualZoneShape.Ellipse, 80.2d, 60.9d, 6.6d, 12.4d),
                new(EmulationControllerVisualControl.SecondaryAction, ControllerVisualZoneShape.Ellipse, 88.8d, 58.5d, 6.7d, 12.5d),
                new(EmulationControllerVisualControl.TertiaryAction, ControllerVisualZoneShape.Ellipse, 78.7d, 45.0d, 6.5d, 12.3d),
                new(EmulationControllerVisualControl.QuaternaryAction, ControllerVisualZoneShape.Ellipse, 87.2d, 42.7d, 6.6d, 12.3d),
                new(EmulationControllerVisualControl.LeftShoulder, ControllerVisualZoneShape.RoundedRectangle, 11.8d, 7.8d, 11.6d, 10.5d),
                new(EmulationControllerVisualControl.RightShoulder, ControllerVisualZoneShape.RoundedRectangle, 76.8d, 7.8d, 11.3d, 10.5d),
                new(EmulationControllerVisualControl.Start, ControllerVisualZoneShape.RoundedRectangle, 59.0d, 75.7d, 9.8d, 5.3d)
            ]);
}
